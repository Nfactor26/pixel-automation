//using Serilog;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.Serialization;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Automation;

//namespace Nish26.WinAutomation.UIA.Components.ActorComponents
//{
//    [DataContract]
//    [Serializable]
//    [ToolBoxItem("Grid Navigator", "UIA", iconSource: null, description: "Move through items in a grid", tags: new string[] { "Grid", "UIA" })]
//    public class GridItemActorComponent : UIAActorComponent
//    {

//        public override void Act()
//        {
//            AutomationElement targetControl = UIAHelpersComponent.FindControl(ControlIdentityComponent);
//            if (targetControl != null)
//            {
//                GridPattern gridPattern = targetControl.GetCurrentPattern(GridPattern.Pattern) as GridPattern;
//                Log.Information("Number of grid rows : {rowCount}", gridPattern.Current.RowCount);
//                Log.Information("Number of grid cols : {colCount}", gridPattern.Current.ColumnCount);

//                AutomationElement a1 = gridPattern.GetItem(0, 0);
//                a1.SetValue("Done");
//            }
//        }
//    }
//}
