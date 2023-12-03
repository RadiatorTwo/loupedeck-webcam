namespace Loupedeck.WebcamPlugin.Actions
{
    using System;

    using AForge.Video.DirectShow;

    using Loupedeck.WebcamPlugin.Models;

    public class FocusAdjustment : ActionEditorAdjustment
    {
        private Int32 currentFocus;
        private VideoCaptureDevice _videoSource;

        private Int32 minValue;
        private Int32 maxValue;
        private Int32 defaultValue;
        private Int32 stepSize;

        private const String ListboxControlName = "cam";

        public FocusAdjustment()
            : base(hasReset: false)
        {
            this.DisplayName = "Webcam Focus";
            this.Description = "Set the Focus for the selected Webcam";

            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(name: ListboxControlName, labelText: "Webcam:"));

            this.ActionEditor.ListboxItemsRequested += this.OnActionEditorListboxItemsRequested;
            this.ActionEditor.ControlValueChanged += this.OnActionEditorControlValueChanged;

            //if (videoDevices.Count > 0)
            //{

            //    var cameraControlFlags = CameraControlFlags.Manual;

            //    this.videoSource.GetCameraPropertyRange(CameraControlProperty.Focus, out this.minValue, out this.maxValue, out this.defaultValue, out this.stepSize, out cameraControlFlags);
            //    this.videoSource.GetCameraProperty(CameraControlProperty.Focus, out this.currentFocus, out cameraControlFlags);
            //}
        }

        private void OnActionEditorListboxItemsRequested(Object sender, ActionEditorListboxItemsRequestedEventArgs e)
        {
            ActionHelpers.FillListBox(e, ListboxControlName, () =>
            {
                var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                foreach (FilterInfo device in videoDevices)
                {
                    e.AddItem(name: device.Name, device.Name, String.Empty);
                }
            });
        }

        private void OnActionEditorControlValueChanged(Object sender, ActionEditorControlValueChangedEventArgs e)
        {
            if (e.ControlName.EqualsNoCase(ListboxControlName))
            {
                e.ActionEditorState.SetDisplayName($"Set Focus for Webcam {e.ActionEditorState.GetControlValue(ListboxControlName)}");
            }
        }

        protected override Boolean ApplyAdjustment(ActionEditorActionParameters actionParameters, Int32 diff)
        {
            this.LoadWebcam(actionParameters);

            if (diff < 0)
            {
                if (this.currentFocus + diff < this.minValue)
                {
                    this.currentFocus = this.minValue;
                }
                else
                {
                    this.currentFocus += diff;
                }
            }
            else if (diff > 0)
            {
                if (this.currentFocus + diff > this.maxValue)
                {
                    this.currentFocus = this.maxValue;
                }
                else
                {
                    this.currentFocus += diff;
                }
            }

            this.SetFocus();
            this.AdjustmentValueChanged();

            return true;
        }

        private void LoadWebcam(ActionEditorActionParameters parameter)
        {
            if (this._videoSource == null)
            {
                var devicename = parameter.Parameters[ListboxControlName];
                var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                foreach (FilterInfo device in videoDevices)
                {
                    if (device.Name.Equals(devicename))
                    {
                        this._videoSource = new VideoCaptureDevice(device.MonikerString);
                        var cameraControlFlags = CameraControlFlags.Manual;

                        this._videoSource.GetCameraPropertyRange(CameraControlProperty.Focus, out this.minValue, out this.maxValue, out this.defaultValue, out this.stepSize, out cameraControlFlags);
                        this._videoSource.GetCameraProperty(CameraControlProperty.Focus, out this.currentFocus, out cameraControlFlags);

                        return;
                    }
                }
            }
        }
        
        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            this.LoadWebcam(actionParameters);

            this.currentFocus = this.defaultValue;
            this.SetFocus();
            this.AdjustmentValueChanged();

            return true;
        }

        private void SetFocus() => this._videoSource.SetCameraProperty(CameraControlProperty.Focus, this.currentFocus, CameraControlFlags.Manual);

        protected override String GetAdjustmentDisplayName(ActionEditorActionParameters actionParameters) => this.currentFocus.ToString();
    }
}
