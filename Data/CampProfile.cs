using AutoMapper;
using CodeCampApi.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeCampApi.Models
{
    public class CampProfile : Profile
    {
        public CampProfile()
        {
            this.CreateMap<Camp, CampModel>()
                .ForMember(c => c.Venue, o => o.MapFrom(m => m.Location.VenueName))
                .ReverseMap();

            this.CreateMap<Talk, TalkModel>()
                .ReverseMap();

            this.CreateMap<Speaker, SpeakerModel>()
                .ReverseMap();
        }
    }
}
