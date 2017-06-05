using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CognitiveMissionMars.Services;
using Microsoft.ProjectOxford.Face;

namespace CognitiveMissionMars
{
    public partial class MainWindow : Window
    {
        private readonly FaceServiceClient _faceServiceClient =
            // TODO Here put your Face API Key
            new FaceServiceClient("Your Face API Key", "https://westeurope.api.cognitive.microsoft.com/face/v1.0");

        private readonly Typeface _typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        private const string CrewPhotoPath = "../../Assets/CrewPhoto.jpg";
        private const string PersonGroupId = "missiontomarscrew";
        private const string PersonGroupName = "Mission Crew";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs args)
        {
            try
            {
                IdentifyButton.IsEnabled = false;
                ProgressTextBlock.Text = "Identifying Crew Members...";
                ProgressGrid.Visibility = Visibility.Visible;

                // Train the Person Group
                await _faceServiceClient.TrainPersonGroupAsync(PersonGroupId);

                var bitmapSource = new BitmapImage();

                bitmapSource.BeginInit();
                bitmapSource.CacheOption = BitmapCacheOption.None;
                bitmapSource.UriSource = new Uri(CrewPhotoPath, UriKind.Relative);
                bitmapSource.EndInit();

                var drawingVisual = new DrawingVisual();
                var drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawImage(bitmapSource, new Rect(0, 0, bitmapSource.Width, bitmapSource.Height));
                var dpi = bitmapSource.DpiX;
                var resizeFactor = 96 / dpi;

                using (var crewPhotoStream = File.OpenRead(CrewPhotoPath))
                {
                    // Get detected Faces
                    var faces = await _faceServiceClient.DetectAsync(crewPhotoStream);
                    var faceIds = faces.Select(face => face.FaceId).ToArray();

                    // Identify faces in the Person Group
                    var results = await _faceServiceClient.IdentifyAsync(PersonGroupId, faceIds);
                    foreach (var identifyResult in results)
                    {
                        var isIdentified = identifyResult.Candidates.Length != 0;

                        // Get the best fit
                        var targetFace = faces.FirstOrDefault(f => f.FaceId == identifyResult.FaceId);
                        if (targetFace == null)
                        {
                            continue;
                        }
                        // Get the face rectangle
                        var faceRect = targetFace.FaceRectangle;

                        // Draw the face rectangle
                        drawingContext.DrawRectangle(
                            Brushes.Transparent,
                            new Pen(isIdentified ? Brushes.Green : Brushes.Red, 4),
                            new Rect(
                                faceRect.Left * resizeFactor,
                                faceRect.Top * resizeFactor,
                                faceRect.Width * resizeFactor,
                                faceRect.Height * resizeFactor
                            )
                        );

                        // Draw the rectangle containing the Person Name
                        drawingContext.DrawRectangle(
                            isIdentified ? Brushes.Green : Brushes.Red,
                            new Pen(isIdentified ? Brushes.Green : Brushes.Red, 4),
                            new Rect(
                                faceRect.Left * resizeFactor,
                                faceRect.Top * resizeFactor - 20,
                                faceRect.Width * resizeFactor,
                                resizeFactor + 20
                            )
                        );
                        string name;
                        if (isIdentified)
                        {
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var person = await _faceServiceClient.GetPersonAsync(PersonGroupId, candidateId);
                            name = person.Name;
                        }
                        else
                        {
                            name = "Alien";
                        }

                        var formattedText = new FormattedText(name, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, _typeface, 16, Brushes.White);

                        // Draw the Name
                        drawingContext.DrawText(formattedText, new Point(faceRect.Left * resizeFactor + 4, faceRect.Top * resizeFactor - 20));
                    }
                }

                drawingContext.Close();

                var faceWithRectBitmap = new RenderTargetBitmap(
                    (int)(bitmapSource.PixelWidth * resizeFactor),
                    (int)(bitmapSource.PixelHeight * resizeFactor),
                    96,
                    96,
                    PixelFormats.Pbgra32);

                faceWithRectBitmap.Render(drawingVisual);
                CrewPhoto.Source = faceWithRectBitmap;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                IdentifyButton.IsEnabled = true;
                ProgressGrid.Visibility = Visibility.Collapsed;
            }
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs args)
        {
            // Here we train the cognitive Service endpoint
            try
            {
                try
                {
                    // Delete previously created PersonGroup
                    await _faceServiceClient.DeletePersonGroupAsync(PersonGroupId);
                }
                catch
                {
                    // Ignored
                }

                // Create the PersonGroup
                await _faceServiceClient.CreatePersonGroupAsync(PersonGroupId, PersonGroupName);

                foreach (var imagePath in Directory.GetFiles(@"../../Assets/CrewPhotos", "*.jpg"))
                {
                    var member = CrewRepository.FromPhoto(Path.GetFileName(imagePath));
                    // Create the Person
                    var personResult = await _faceServiceClient.CreatePersonAsync(PersonGroupId, name: member.Name);

                    using (Stream s = File.OpenRead(imagePath))
                    {
                        // Add the Person to the Group
                        await _faceServiceClient.AddPersonFaceAsync(PersonGroupId, personResult.PersonId, s);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                IdentifyButton.IsEnabled = true;
                ProgressGrid.Visibility = Visibility.Collapsed;
            }
        }
    }
}
