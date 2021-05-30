using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Response
{
    [DataContract]
    public class PagedList<T>
    {
        /// <summary>
        /// Items for current page
        /// </summary>
        [DataMember]
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Total number of available items in database
        /// </summary>
        public int ItemsCount { get; set; }

        /// <summary>
        /// Current Page
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// Desired Item count per page
        /// </summary>
        public int PageSize { get; set; }
           
        public PagedList()
        {

        }

        public PagedList(IEnumerable<T> items, int totalCount, int currentPage, int pageSize)
        {
            Items.AddRange(items);
            PageSize = pageSize;
            CurrentPage = currentPage;
            ItemsCount = totalCount;
            PageCount = (int)Math.Ceiling(totalCount / (double)pageSize);
        }      
    }
}
