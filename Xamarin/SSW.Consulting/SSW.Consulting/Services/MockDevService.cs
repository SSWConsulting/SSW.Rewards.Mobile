using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SSW.Consulting.Models;

namespace SSW.Consulting.Services
{
    public class MockDevService : IDevService
    {
        private List<DevProfile> _profiles;

        public MockDevService()
        {
            _profiles = new List<DevProfile>();
        }

        private string jeanBio = @"Scrum Master for internal development teams, the design, video, and marketing teams, as well as client projects. Integrity, transparency, and a smooth running team are all very important to him. He's also a Power BI guru. 

In his spare time, he is one of Australia's top Salsa teachers and is the 5 times Australian Salsa Champion, 5 times Australian Bachata Champion, and part of the Bachata World Champion team";

        public async Task<IEnumerable<DevProfile>> GetProfilesAsync()
        {
            var mockDevs = new List<DevProfile>
            {
                new DevProfile
                {
                    FirstName = "Jean",
                    LastName="Thiron",
                    Title = "Solution Architect",
                    Picture = "jeanndc",
                    Skills = new List<DevSkills> { DevSkills.Angular, DevSkills.Beer, DevSkills.DevOps, DevSkills.Smoking },
                    TwitterID = "Jean_SSW",
                    Bio = jeanBio,
                    id = 1,
                    Email = "jeanthiron@ssw.com.au",
                    Phone = "0411222333"
                },
                new DevProfile
                {
                    FirstName = "Adam",
                    LastName="Cogan",
                    Title = "Chief Architect",
                    Picture = "adamndc",
                    Skills = new List<DevSkills> { DevSkills.Angular, DevSkills.PowerBI, DevSkills.NETCore, DevSkills.DevOps },
                    TwitterID = "AdamCogan",
                    Bio = jeanBio,
                    id = 2,
                    Email = "adam@ssw.com.au",
                    Phone = "0455666777"
                },
                new DevProfile
                {
                    FirstName = "Uly",
                    LastName="Maclaren",
                    Title = "General Manager",
                    Picture = "uly_ndc",
                    Skills = new List<DevSkills> { DevSkills.PowerBI, DevSkills.Dancing, DevSkills.DevOps },
                    TwitterID = "ulyssesmac",
                    Bio = jeanBio,
                    id = 3,
                    Email = "ulyssesmaclaren@ssw.com.au",
                    Phone = "0433666999"
                }
            };

            _profiles = mockDevs;

            return await Task.FromResult(_profiles);
        }
    }
}
