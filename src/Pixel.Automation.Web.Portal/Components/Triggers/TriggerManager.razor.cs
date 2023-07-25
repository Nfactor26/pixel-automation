using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Web.Portal.Components.Triggers
{
    public partial class TriggerManager : ComponentBase
    {
        [Inject]
        public ISnackbar SnackBar { get; set; }

        [Parameter]
        public IEnumerable<SessionTrigger> Triggers { get; set; }

        [Parameter]
        public EventCallback OnAddItem { get; set; }

        [Parameter]
        public EventCallback<SessionTrigger> OnDeleteItem { get; set; }

        [Parameter]
        public EventCallback<SessionTrigger> OnPauseItem { get; set; }      

        [Parameter]
        public EventCallback<SessionTrigger> OnResumeItem { get; set; }

        [Parameter]
        public EventCallback OnPauseAll { get; set; }

        [Parameter]
        public EventCallback OnResumeAll { get; set; }

        [Parameter]
        public EventCallback<SessionTrigger> OnTriggerNow { get; set; }

        [Parameter]
        public EventCallback<SessionTrigger> OnUpdateItem { get; set; }

        private MudTable<SessionTrigger> table;
        private SessionTrigger selectedTrigger = null;
        private SessionTrigger elementBeforeEdit;
        private string searchString = "";
      
        /// <summary>
        /// Filter items based on search string
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool FilterFunc(SessionTrigger element)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;    
            if(element.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.Handler.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }       
    }
}
