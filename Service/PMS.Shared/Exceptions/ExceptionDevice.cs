using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Exceptions
{
    public class ExceptionDeviceInvalid : BaseException
    {
        public ExceptionDeviceInvalid(string deviceId, string userName) : base($"Device {deviceId}-{userName} is not registered yet")
        {
            _code = 2001;
        }
    }

    public class ExceptionDeviceOnHold : BaseException
    {
        public ExceptionDeviceOnHold(string deviceId,string userName) : base($"Device {deviceId}-{userName} is on hold because of approval process")
        {
            _code = 2002;
        }
    }

    public class ExceptionDeviceBlocked : BaseException
    {
        public ExceptionDeviceBlocked(string deviceId,string userName) : base($"Device {deviceId}-{userName} is blocked, please contact administrator")
        {
            _code = 2003;
        }
    }

    public class ExceptionDeviceNeedPin : BaseException
    {
        public ExceptionDeviceNeedPin() : base($"Please input second authentication code")
        {
            _code = 2004;
        }
    }
}
