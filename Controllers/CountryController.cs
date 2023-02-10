﻿using AutoMapper;
using HotelListingAPI.Data;
using HotelListingAPI.Models;
using HotelListingAPI.UOW;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelListingAPI.Controllers
{
    [Route("api/[controller]")]
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

        [HttpGet("id", Name = "GetCountry")]
        public async Task<IActionResult> GetCountry(int id)
        {
            if (id < 1) return BadRequest();

            try
            {
                var country = await _uow.Countries.GetAsync(q => q.Id == id, new List<string> { "Hotels" });
                if (country != null)
                {
                    var result = _mapper.Map<Country>(country);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Source, ex.Message);
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

        [HttpPut("id")]
        public async Task<IActionResult> UpdateCountry(int id, [FromBody] UpdateCountryDTO updateCountry)
        {
            if (!ModelState.IsValid || id < 1) return BadRequest();

            try
            {
                var country = await _uow.Countries.GetAsync(q => q.Id == id);
                if (country != null)
                {
                    _mapper.Map(updateCountry, country);
                    _uow.Countries.Update(country);
                    await _uow.SaveAsync();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Source, ex.Message);
            }

            return BadRequest();
        }

        [HttpDelete("id")]
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
                    return RedirectToAction("GetAllCountries");
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