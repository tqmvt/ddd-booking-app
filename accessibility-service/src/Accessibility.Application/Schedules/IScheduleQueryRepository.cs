using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Accessibility.Application.Schedules.Queries;

namespace Accessibility.Application.Schedules
{
    public interface IScheduleQueryRepository
    {
        Task<IEnumerable<EmployeeAvailability>> GetAllAvailabilities(DateTime dateFrom, DateTime dateTo, Guid facilityId);
        Task<IEnumerable<ScheduleDto>> GetSchedules(DateTime dateFrom, DateTime dateTo, Guid facilityId);
    }
}