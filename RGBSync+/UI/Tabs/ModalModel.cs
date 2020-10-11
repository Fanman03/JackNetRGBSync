namespace RGBSyncPlus.UI.Tabs
{
    internal class ModalModel
    {
        public string ModalText { get; set; }
        public bool ShowModalTextBox { get; set; }
        public bool ShowModalCloseButton { get; set; }
        public System.Action<string> modalSubmitAction { get; set; }
    }
}