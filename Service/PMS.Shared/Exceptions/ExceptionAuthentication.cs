using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Exceptions
{
    public class ExceptionInActiveUser:BaseException
    {
        
        public ExceptionInActiveUser(string userName) : base("User " + userName + " tidak aktif, silakan hubungi departemen IT")
        {
            _code = 1001;
        }
    }

    public class ExceptionInvalidUser : BaseException
    {
        public ExceptionInvalidUser(string userName) : base("User " + userName + " tidak ditemukan atau password tidak valid")
        {
            _code = 1002;
        }
    }

    public class ExceptionLockedUser : BaseException
    {
        public ExceptionLockedUser(string userName) : base("User " + userName + " terkunci, silakan hubungi departemen IT")
        {
            _code = 1003;
        }
    }
    public class ExceptionUserNoEmail : BaseException
    {
        public ExceptionUserNoEmail(string userName) : base($"The user {userName} tidak memiliki alamat email yang valid, silakan hubungi departemen IT")
        {
            _code = 1004;
        }
    }

    public class ExceptionUserExpiredPassword : BaseException
    {
        public ExceptionUserExpiredPassword() : base("Password kadaluarsa, silakan hubungi departemen IT")
        {
            _code = 1005;
        }
    }

    public class ExceptionPasswordNotComply : BaseException
    {
        public ExceptionPasswordNotComply(string explanation) : base("Password tidak sesuai dengan policy :\r\n" + explanation)
        {
            _code = 1006;
        }
    }

    public class ExceptionInvalidToken : BaseException
    {
        public ExceptionInvalidToken() : base("Sesi login kadaluarsa, silakan login ulang")
        {
            _code = 1011;
        }
    }

    public class ExceptionUserMustResetPassword : BaseException
    {
        public ExceptionUserMustResetPassword() : base("Mohon reset password sebelum mengakses system")
        {
            _code = 1007;
        }
    }
}
