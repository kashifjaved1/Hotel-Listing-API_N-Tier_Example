﻿using AutoMapper;
using HotelListingAPI.DAL;
using HotelListingAPI.Models;
using HotelListingAPI.UOW;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelListingAPI.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/[controller]/[action]")] // To resolve "Actions require unique method/path combination for Swagger".
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CountryController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateCountry([FromBody] CreateCountryDTO createCountry)
        {
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                var country = _mapper.Map<Country>(createCountry);
                await _uow.Countries.InsertAsync(country);
                await _uow.SaveAsync();

                return CreatedAtRoute("GetCountry", new { id = country.Id }, country);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Source, ex.Message);
            }

            return BadRequest();
        }

        [HttpGet("{id}", Name = "GetCountry")]
        //[ResponseCache(Duration = 60)] // set controllerAction level cache with duration/max-age of 60 seconds.
        // overriding global cache handler setting for actionMethod explicitly.
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 60)]
        [HttpCacheValidation(MustRevalidate = false)]
        public async Task<IActionResult> GetCountry(int id)
        {
            //throw new Exception(); // did to test global error handler

            // removed manual try-catch block because now global error handling is applied.
            if (id < 1) return BadRequest();

            //var country = await _uow.Countries.GetAsync(q => q.Id == id, new List<string> { "Hotels" });
            var country = await _uow.Countries.GetAsync(q => q.Id == id, include: q => q.Include(x => x.Hotels));
            if (country != null)
            {
                var result = _mapper.Map<Country>(country);
                return Ok(result);
            }

            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCountries()
        {
            try
            {
                var countries = await _uow.Countries.GetAllAsync();
                var result = _mapper.Map<List<Country>>(countries);

                return Ok(result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Source, ex.Message);
            }

            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetCountriesPagedList([FromQuery] RequestParams requestParams)
        {
            if(!ModelState.IsValid) return BadRequest();

            try
            {
                var countries = await _uow.Countries.GetPagedListAsync(requestParams);
                var result = _mapper.Map<List<Country>>(countries);

                return Ok(result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Source, ex.Message);
            }

            return BadRequest();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCountry(int id, [FromBody] UpdateCountryDTO updateCountry)
        {
            if (!ModelState.IsValid || id < 1) return BadRequest();

            try
            {
                var country = await _uow.Countries.GetAsync(q => q.Id == id);
                if (country == null)
                {
                    return BadRequest();
                }

                _mapper.Map(updateCountry, country);
                _uow.Countries.Update(country);
                await _uow.SaveAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Source, ex.Message);
            }

            return BadRequest();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            if (id < 1) return BadRequest();

            try
            {
                var country = await _uow.Countries.GetAsync(q => q.Id == id); 
                if(country != null)
                {
                    await _uow.Countries.DeleteAsync(id);
                    await _uow.SaveAsync();
                    return Ok("Record Deleted Successfully");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return BadRequest();
        }
    }
}
