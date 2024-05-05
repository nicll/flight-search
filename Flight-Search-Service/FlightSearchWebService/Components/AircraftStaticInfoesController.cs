using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightDatabaseImportService;
using FlightSearchService.Models.Internal;
using FlightSearchWebService.Components.Model;

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
        public async Task<ActionResult<IEnumerable<AircraftCombined>>> GetAircraftStaticInfos()
        {
            List<AircraftCombined> CombinedList = new List<AircraftCombined>();
            var StaticList = await _context.AircraftStaticInfos.ToListAsync();
            var DynamicList = await _context.AircraftDynamicInfos.ToListAsync();

            for (int i = 0; i < StaticList.Count; i++)
            {
                CombinedList.Add(new AircraftCombined()
                {
                    CallSign = StaticList[i].CallSign,
                    LastUpdated = StaticList[i].LastUpdated.ToString().Substring(0, 19),
                    Icao24 = StaticList[i].Icao24,
                    Latitude = DynamicList[i].Latitude.ToString().Substring(0,10),
                    Longitude = DynamicList[i].Longitude.ToString().Substring(0, 10),
                    Altitude = DynamicList[i].Altitude.ToString().Substring(0, 10),
                    Direction = DynamicList[i].Direction.ToString().Substring(0, 10)
                });
            }

            return CombinedList;
        }

        //// GET: api/AircraftStaticInfoes/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<AircraftStaticInfo>> GetAircraftStaticInfo(string id)
        //{
        //    var aircraftStaticInfo = await _context.AircraftStaticInfos.FindAsync(id);

        //    if (aircraftStaticInfo == null)
        //    {
        //        return NotFound();
        //    }

        //    return aircraftStaticInfo;
        //}

   
    }
}
