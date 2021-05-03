using System;
using SensorStandard.Core;
using SensorStandard.Core.MrsTypes;

namespace MrsDeviceManager.Core
{
    internal class MarsService : SNSR_STDSOAPPort
    {
        public IAsyncResult BegindoCommandMessage(doCommandMessageRequest request, AsyncCallback callback, object asyncState)
        {
            return CommandMessage?.BeginInvoke(this, request.CommandMessage, callback, asyncState);
        }

        public IAsyncResult BegindoDeviceConfiguration(doDeviceConfigurationRequest request, AsyncCallback callback, object asyncState)
        {
            return DeviceConfiguration?.BeginInvoke(this, request.DeviceConfiguration, callback, asyncState);
        }

        public IAsyncResult BegindoDeviceIndicationReport(doDeviceIndicationReportRequest request, AsyncCallback callback, object asyncState)
        {
            return DeviceIndication?.BeginInvoke(this, request.DeviceIndicationReport, callback, asyncState);
        }

        public IAsyncResult BegindoDeviceStatusReport(doDeviceStatusReportRequest request, AsyncCallback callback, object asyncState)
        {
            return DeviceStatus?.BeginInvoke(this, request.DeviceStatusReport, callback, asyncState);
        }

        public IAsyncResult BegindoDeviceSubscriptionConfiguration(doDeviceSubscriptionConfigurationRequest request, AsyncCallback callback, object asyncState)
        {
            return DeviceSubscription?.BeginInvoke(this, request.DeviceSubscriptionConfiguration, callback, asyncState);
        }

        public doCommandMessageResponse doCommandMessage(doCommandMessageRequest request)
        {
            CommandMessage?.Invoke(this, request.CommandMessage);
            return new doCommandMessageResponse();
        }

        public doDeviceConfigurationResponse doDeviceConfiguration(doDeviceConfigurationRequest request)
        {
            DeviceConfiguration?.Invoke(this, request.DeviceConfiguration);
            return new doDeviceConfigurationResponse();
        }

        public doCommandMessageResponse doDeviceIndicationReport(doDeviceIndicationReportRequest request)
        {
            DeviceIndication?.Invoke(this, request.DeviceIndicationReport);
            return new doCommandMessageResponse();
        }

        public doCommandMessageResponse doDeviceStatusReport(doDeviceStatusReportRequest request)
        {
            DeviceStatus?.Invoke(this, request.DeviceStatusReport);
            return new doCommandMessageResponse();
        }

        public doDeviceSubscriptionConfigurationResponse doDeviceSubscriptionConfiguration(doDeviceSubscriptionConfigurationRequest request)
        {
            DeviceSubscription?.Invoke(this, request.DeviceSubscriptionConfiguration);
            return new doDeviceSubscriptionConfigurationResponse();
        }

        public doCommandMessageResponse EnddoCommandMessage(IAsyncResult result)
        {
            CommandMessage?.EndInvoke(result);
            return new doCommandMessageResponse();
        }

        public doDeviceConfigurationResponse EnddoDeviceConfiguration(IAsyncResult result)
        {
            DeviceConfiguration?.EndInvoke(result);
            return new doDeviceConfigurationResponse();
        }

        public doCommandMessageResponse EnddoDeviceIndicationReport(IAsyncResult result)
        {
            DeviceIndication?.EndInvoke(result);
            return new doCommandMessageResponse();
        }

        public doCommandMessageResponse EnddoDeviceStatusReport(IAsyncResult result)
        {
            DeviceStatus?.EndInvoke(result);
            return new doCommandMessageResponse();
        }

        public doDeviceSubscriptionConfigurationResponse EnddoDeviceSubscriptionConfiguration(IAsyncResult result)
        {
            DeviceSubscription?.EndInvoke(result);
            return new doDeviceSubscriptionConfigurationResponse();
        }

        public event EventHandler<DeviceConfiguration> DeviceConfiguration;
        public event EventHandler<DeviceSubscriptionConfiguration> DeviceSubscription;
        public event EventHandler<DeviceStatusReport> DeviceStatus;
        public event EventHandler<DeviceIndicationReport> DeviceIndication;
        public event EventHandler<CommandMessage> CommandMessage;
    }
}
