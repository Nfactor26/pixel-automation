using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Pixel.Persistence.Core.Models;

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
        public Func<SessionTrigger, SessionTrigger, Task<bool>> OnUpdateItem { get; set; }

        private MudTable<SessionTrigger> table;
        private SessionTrigger selectedTrigger = null;
        private SessionTrigger elementBeforeEdit;
        private string searchString = "";

        /// <summary>
        /// Set row in editing mode
        /// </summary>
        /// <param name="model"></param>
        void EditItem(SessionTrigger model)
        {
            selectedTrigger = model;
            table.SetEditingItem(model);
            BackupItem(model);
        }

        /// <summary>
        /// Apply the changes done on edit row
        /// </summary>
        /// <returns></returns>
        async Task UpdateItemAsync()
        {
            var success = await this.OnUpdateItem(elementBeforeEdit, selectedTrigger);
            if (!success)
            {
                ResetItemToOriginalValues(selectedTrigger);
            }
        }

        /// <summary>
        /// Backup item before start of edit
        /// </summary>
        /// <param name="element"></param>
        void BackupItem(object element)
        {
            if (element is ICloneable beforeEdit)
            {
                elementBeforeEdit = beforeEdit.Clone() as SessionTrigger;
            }

        }

        /// <summary>
        /// Reset item to original values if edit was cancelled
        /// </summary>
        /// <param name="element"></param>
        void ResetItemToOriginalValues(object element)
        {
            if(element is CronSessionTrigger sessionTrigger && elementBeforeEdit is CronSessionTrigger ebe)
            {
                sessionTrigger.Name = ebe.Name;
                sessionTrigger.Handler = ebe.Handler;
                sessionTrigger.Group = ebe.Group;                
                sessionTrigger.CronExpression = ebe.CronExpression;
            }           
        }

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
