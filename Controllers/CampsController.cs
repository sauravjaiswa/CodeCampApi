using AutoMapper;
using CodeCampApi.Data;
using CodeCampApi.Data.Entities;
using CodeCampApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeCampApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository campRepository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public CampsController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.campRepository = campRepository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var results = await campRepository.GetAllCampsAsync(includeTalks);

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

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await campRepository.GetAllCampsByEventDate(theDate, includeTalks);

                if (results == null)
                {
                    return NotFound();
                }

                CampModel[] model = mapper.Map<CampModel[]>(results);

                return model;
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var existing = await campRepository.GetCampAsync(model.Moniker);
                if (existing != null)
                {
                    return BadRequest("Moniker in use");
                }

                var location = linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });

                if (string.IsNullOrEmpty(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                var camp = mapper.Map<Camp>(model);
                campRepository.Add(camp);

                if(await campRepository.SaveChangesAsync())
                {
                    return Created($"/api/camps/{camp.Moniker}", mapper.Map<CampModel>(camp));
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model)
        {
            try
            {
                var existing = await campRepository.GetCampAsync(moniker);
                if (existing == null)
                {
                    return NotFound($"Could not find camp with moniker {moniker}");
                }

                mapper.Map(model, existing);

                if (await campRepository.SaveChangesAsync())
                {
                    return mapper.Map<CampModel>(existing);
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var existing = await campRepository.GetCampAsync(moniker);
                if (existing == null)
                {
                    return NotFound($"Could not find camp with moniker {moniker}");
                }

                campRepository.Delete(existing);

                if (await campRepository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }
    }
}
