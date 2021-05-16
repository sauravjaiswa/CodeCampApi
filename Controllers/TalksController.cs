﻿using AutoMapper;
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
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository repository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public TalksController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talks = await repository.GetTalksByMonikerAsync(moniker, true);

                return mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                var talk = await repository.GetTalkByMonikerAsync(moniker, id, true);

                return mapper.Map<TalkModel>(talk);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model)
        {
            try
            {
                var camp = await repository.GetCampAsync(moniker);
                if (camp == null)
                {
                    return NotFound("Camp does not exist");
                }

                var talk = mapper.Map<Talk>(model);
                talk.Camp = camp;

                if (model.Speaker == null)
                {
                    return BadRequest("Speaker ID is required");
                }
                var speaker = await repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null)
                {
                    return BadRequest("Speaker could not be found");
                }
                talk.Speaker = speaker;

                repository.Add(talk);

                if(await repository.SaveChangesAsync())
                {
                    var url = linkGenerator.GetPathByAction(HttpContext,
                        "Get",
                        values: new { moniker, id = talk.TalkId });

                    return Created(url, mapper.Map<TalkModel>(talk));
                }
                else
                {
                    return BadRequest("Failed to save new talk");
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id, TalkModel model)
        {
            try
            {
                var talk = await repository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null)
                {
                    return NotFound("Couldn't find the talk");
                }

                mapper.Map(model, talk);
                
                if (model.Speaker != null)
                {
                    var speaker = await repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }

                if (await repository.SaveChangesAsync())
                {
                    return mapper.Map<TalkModel>(talk);
                }
                else
                {
                    return BadRequest("Failed to update");
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var talk = await repository.GetTalkByMonikerAsync(moniker, id);
                if (talk == null)
                {
                    return NotFound("Couldn't find the talk");
                }

                repository.Delete(talk);

                if (await repository.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Failed to delete");
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
