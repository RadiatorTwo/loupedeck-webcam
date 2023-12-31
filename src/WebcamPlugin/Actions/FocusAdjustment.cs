﻿namespace Loupedeck.WebcamPlugin.Actions
{
    using System;
    using AForge.Video.DirectShow;
    using Loupedeck.WebcamPlugin.Models;

    public class FocusAdjustment : ActionEditorAdjustment
    {
        private Int32 currentValue;
        private VideoCaptureDevice _videoSource;

        private Int32 minValue;
        private Int32 maxValue;
        private Int32 defaultValue;
        private Int32 stepSize;

        private const String ListboxControlName = "cam";

        public FocusAdjustment()
            : base(hasReset: false)
        {
            this.DisplayName = "Focus";
            this.Description = "Set the Focus for the selected Webcam";

            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(name: ListboxControlName, labelText: "Webcam:"));

            this.ActionEditor.ListboxItemsRequested += this.OnActionEditorListboxItemsRequested;
            this.ActionEditor.ControlValueChanged += this.OnActionEditorControlValueChanged;
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
                if (this.currentValue + diff < this.minValue)
                {
                    this.currentValue = this.minValue;
                }
                else
                {
                    this.currentValue += diff;
                }
            }
            else if (diff > 0)
            {
                if (this.currentValue + diff > this.maxValue)
                {
                    this.currentValue = this.maxValue;
                }
                else
                {
                    this.currentValue += diff;
                }
            }

            this.SetValue();
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
                        this._videoSource.GetCameraProperty(CameraControlProperty.Focus, out this.currentValue, out cameraControlFlags);

                        return;
                    }
                }
            }
        }
        
        private void SetValue() => this._videoSource.SetCameraProperty(CameraControlProperty.Focus, this.currentValue, CameraControlFlags.Manual);

        protected override String GetAdjustmentDisplayName(ActionEditorActionParameters actionParameters) => this.currentValue.ToString();
    }
}
