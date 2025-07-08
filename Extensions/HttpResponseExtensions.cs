using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cleanApi.Extensions
{
    public class HttpResponseExtensions
    {
            public static void AddPaginationHeader(this HttpResponse response, int currentPage, int pageSize, int totalItems, int totalPages)
        {
            var paginationHeader = new
            {
                currentPage,
                pageSize,
                totalItems,
                totalPages
            };

            response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationHeader));
            response.Headers.Add("Access-Control-Expose-Headers", "X-Pagination");
        }
    }
}