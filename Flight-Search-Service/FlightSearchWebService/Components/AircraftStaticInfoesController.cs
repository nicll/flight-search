using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightDatabaseImportService;
using FlightSearchService.Models.Internal;

namespace FlightSearchWebService.Components
{
    [Route("api/[controller]")]
    [ApiController]
    public class AircraftStaticInfoesController : ControllerBase
    {
        private readonly FlightDbContext _context;

        public AircraftStaticInfoesController(FlightDbContext context)
        {
            _context = context;
        }

        // GET: api/AircraftStaticInfoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AircraftStaticInfo>>> GetAircraftStaticInfos()
        {
            return await _context.AircraftStaticInfos.ToListAsync();
        }

        // GET: api/AircraftStaticInfoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AircraftStaticInfo>> GetAircraftStaticInfo(string id)
        {
            var aircraftStaticInfo = await _context.AircraftStaticInfos.FindAsync(id);

            if (aircraftStaticInfo == null)
            {
                return NotFound();
            }

            return aircraftStaticInfo;
        }

   
    }
}
