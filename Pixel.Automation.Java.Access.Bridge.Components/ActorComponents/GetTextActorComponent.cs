using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Get Text", "Java", iconSource: null, description: "Get text contents of a control", tags: new string[] { "Get Text", "Java" })]

    public class GetTextActorComponent : JABActorComponent
    {
        [DataMember]
        [Description("Store the value in Result Argument")]
        public Argument Result { get; set; } = new OutArgument<string>();

        public GetTextActorComponent() : base("Get Text", "GetText")
        {

        }

        public override void Act()
        {          
            AccessibleContextNode targetControl = this.GetTargetControl();
            var info = targetControl.GetInfo();
            if ((info.accessibleInterfaces & AccessibleInterfaces.cAccessibleTextInterface) != 0)
            {
                var point = new System.Drawing.Point(0, 0);
                AccessibleTextInfo textInfo;
                targetControl.AccessBridge.Functions.GetAccessibleTextInfo(targetControl.JvmId, targetControl.AccessibleContextHandle, out textInfo, point.X, point.Y);
                var reader = new AccessibleTextReader(targetControl, textInfo.charCount);
                var lines = reader
                  .ReadFullLines(targetControl.AccessBridge.TextLineLengthLimit)
                  .Where(x => !x.IsContinuation)
                  .Take(targetControl.AccessBridge.TextLineCountLimit);
                StringBuilder sb = new StringBuilder();
                foreach (var lineData in lines)
                {                   
                    sb.Append(lineData.Text);
                }
                this.ArgumentProcessor.SetValue<string>(this.Result, sb.ToString());               
            }
            else
            {
                throw new InvalidOperationException($"Control: {this.TargetControl} doesn't support cAccessibleTextInterface.");
            }
        }

        //private static string MakePrintable(string text)
        //{
        //    var sb = new StringBuilder();
        //    foreach (var ch in text)
        //    {
        //        if (ch == '\n') sb.Append("\\n");
        //        else if (ch == '\r') sb.Append("\\r");
        //        else if (ch == '\t') sb.Append("\\t");
        //        else if (char.IsControl(ch)) sb.Append("#");
        //        else sb.Append(ch);
        //    }
        //    return sb.ToString();
        //}
    }
}
