namespace Loupedeck.WebcamPlugin.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using AForge.Video;
    using AForge.Video.DirectShow;

    public class FocusAdjustment : PluginDynamicAdjustment
    {
        private Int32 currentFocus;
        private readonly VideoCaptureDevice videoSource;

        private readonly Int32 minValue;
        private readonly Int32 maxValue;
        private readonly Int32 defaultValue;
        private readonly Int32 stepSize;

        public FocusAdjustment()
            : base(displayName: "Focus Adjustment", description: "Sets the Webcams Focus", groupName: "Adjustments", hasReset: true)
        {
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count > 0)
            {
                this.videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

                var cameraControlFlags = CameraControlFlags.Manual;

                this.videoSource.GetCameraPropertyRange(CameraControlProperty.Focus, out this.minValue, out this.maxValue, out this.defaultValue, out this.stepSize, out cameraControlFlags);
                this.videoSource.GetCameraProperty(CameraControlProperty.Focus, out this.currentFocus, out cameraControlFlags);
            }
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
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
        }

        protected override void RunCommand(String actionParameter)
        {
            this.currentFocus = this.defaultValue;
            this.SetFocus();
            this.AdjustmentValueChanged();
        }

        private void SetFocus() => this.videoSource.SetCameraProperty(CameraControlProperty.Focus, this.currentFocus, CameraControlFlags.Manual);

        protected override String GetAdjustmentValue(String actionParameter) => this.currentFocus.ToString();
    }
}
