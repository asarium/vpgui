using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VPGUI.Annotations;
using VPSharp;
using log4net;
using VPGUI.Utilities;

namespace VPGUI.Models
{
    public class UpdateStatusModel : INotifyPropertyChanged
    {
        public UpdateStatusModel()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                var deployment = ApplicationDeployment.CurrentDeployment;

                deployment.CheckForUpdateCompleted += DeploymentOnCheckForUpdateCompleted;

                Status = new UpdateCheckStatus();
                deployment.CheckForUpdateAsync();
            }
            else
            {
                Status = new ErrorStatus("This application was not deployed correctly.");
            }
        }

        private void DeploymentOnCheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            if (e.UpdateAvailable)
            {
                var deployment = ApplicationDeployment.CurrentDeployment;

                Status = new UpdatingStatus(this, deployment);

                deployment.UpdateAsync();
            }
            else
            {
                if (e.Error != null)
                {
                    Status = new ErrorStatus("Failed to check for update:\n" + e.Error.Message);
                }
                else
                {
                    Status = new SuccessfullStatus("You are using the latest version.");
                }
            }
        }

        private object _status = null;
        public object Status
        {
            get
            {
                return this._status;
            }

            set
            {
                if (this._status != value)
                {
                    this._status = value;

                    this.OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class UpdatingStatus : INotifyPropertyChanged
    {
        private static readonly ILog Log = LogManager.GetLogger("VPGUI.Models.UpdatingStatus");

        private UpdateStatusModel _updateStatus;

        private ApplicationDeployment _deployment;

        private string _message;
        public string Message
        {
            get { return _message; }

            set 
            {
                _message = value;
            
                OnPropertyChanged();
            }
        }

        private double _maximum;
        public double Maximum
        {
            get
            {
                return _maximum;
            }

            set
            {
                _maximum = value;

                OnPropertyChanged();
            }
        }

        private double _current;
        public double Current
        {
            get
            {
                return _current;
            }

            set
            {
                _current = value;

                OnPropertyChanged();
            }
        }

        private bool _unknownProgress;
        public bool UnknownProgress
        {
            get
            {
                return _unknownProgress;
            }

            set
            {
                _unknownProgress = value;

                OnPropertyChanged();
            }
        }

        public UpdatingStatus(UpdateStatusModel updateStatus, ApplicationDeployment deployment)
        {
            this._updateStatus = updateStatus;
            this._deployment = deployment;

            _deployment.UpdateProgressChanged += DeploymentOnUpdateProgressChanged;
            _deployment.UpdateCompleted += DeploymentOnUpdateCompleted;

            Message = "Updating...";
        }

        private void DeploymentOnUpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            switch (e.State)
            {
                case DeploymentProgressState.DownloadingDeploymentInformation:
                    Message = "Downloading deployment information " + GetDownloadString(e) + "...";
                    break;
                case DeploymentProgressState.DownloadingApplicationInformation:
                    Message = "Downloading application information " + GetDownloadString(e) + "...";
                    break;
                case DeploymentProgressState.DownloadingApplicationFiles:
                    Message = "Downloading application files " + GetDownloadString(e) + "...";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Maximum = e.BytesTotal;
            Current = e.BytesCompleted;

            UnknownProgress = e.BytesTotal <= 0L;

            if (e.State == DeploymentProgressState.DownloadingApplicationFiles && e.BytesTotal == e.BytesCompleted)
            {
                Message = "Installing update...";
                UnknownProgress = true;
            }
        }

        private string GetDownloadString(DeploymentProgressChangedEventArgs e)
        {
            return string.Format("({0} of {1})", e.BytesCompleted.HumanReadableByteCount(false), e.BytesTotal.HumanReadableByteCount(false));
        }

        private void DeploymentOnUpdateCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Log.Info("Exception while updating", e.Error);
                _updateStatus.Status = new ErrorStatus("Failed to update:\n" + e.Error.Message);
            }
            else
            {
                Log.Info("Update complete.");

                var updatedVersion = this._deployment.UpdatedVersion;
                if (updatedVersion != null)
                {
                    this._updateStatus.Status = new SuccessfullStatus("Update to version " + updatedVersion + " was successfull.");
                }
                else
                {
                    this._updateStatus.Status = new SuccessfullStatus("Update was successfull.");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class UpdateCheckStatus
    {
    }

    public class SuccessfullStatus
    {
        public string Message
        { get; private set; }

        public SuccessfullStatus(string message)
        {
            this.Message = message;
        }
    }

    public class ErrorStatus
    {
        public string Message
        {
            get; private set; }

        public ErrorStatus(string message)
        {
            this.Message = message;
        }
    }
}
