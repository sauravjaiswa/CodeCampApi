using AutoMapper;
using CodeCampApi.Data;
using CodeCampApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeCampApi.Controllers
{
    [Route("api/[controller]")]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository campRepository;
        private readonly IMapper mapper;

        public CampsController(ICampRepository campRepository, IMapper mapper)
        {
            this.campRepository = campRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get()
        {
            try
            {
                var results = await campRepository.GetAllCampsAsync();

                CampModel[] models = mapper.Map<CampModel[]>(results);

                return models;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var results = await campRepository.GetCampAsync(moniker);

                if (results == null)
                {
                    return NotFound();
                }

                CampModel model = mapper.Map<CampModel>(results);

                return model;
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
    }
}
