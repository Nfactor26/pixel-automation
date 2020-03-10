namespace Pixel.Automation.Editor.Core.Notfications
{

    /// <summary>
    /// Test Explorer can send a show test data source notification. Test Data Repository will process this notication and apply filter on view to show 
    /// data source with the given test data id.
    /// </summary>
    public class ShowTestDataSourceNotification
    {
        public string TestDataId { get; }

        public  ShowTestDataSourceNotification(string testDataId)
        {
            this.TestDataId = testDataId;
        }
    }
}
