using System.Collections.Generic;
using System.Linq;
using CognitiveMissionMars.Models;

namespace CognitiveMissionMars.Services
{
    public static class CrewRepository
    {
        public static IEnumerable<CrewMember> Members { get; } = new List<CrewMember>()
        {
            new CrewMember
            {
                Name = "Anna",
                Surname = "Malli",
                Position = "Commander",
                Photo = "AnnaMalli.jpg"
            },
            new CrewMember
            {
                Name = "Erika",
                Surname = "Mustermann",
                Position = "Mission Specialist",
                Photo = "ErikaMustermann.jpg"
            },
            new CrewMember
            {
                Name = "Ivan",
                Surname = "Sidorov",
                Position = "Payload Specialist",
                Photo = "IvanSidorov.jpg"
            },
            new CrewMember
            {
                Name = "Juan",
                Surname = "Pérez",
                Position = "Flight Engineer",
                Photo = "JuanPerez.jpg"
            },
            new CrewMember
            {
                Name = "Seán",
                Surname = "Ó Rudaí",
                Position = "Pilot",
                Photo = "SeanORudai.jpg"
            },
            new CrewMember
            {
                Name = "Jean",
                Surname = "Dupont",
                Position = "Payload Commander",
                Photo = "JeanDupont.jpg"
            },
        };

        public static CrewMember FromPhoto(string photoPath)
        {
            return Members.Single(m => m.Photo == photoPath);
        }
    }
}