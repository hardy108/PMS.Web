using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;
using PMS.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.GL
{
    public class AccountType : EntityFactory<MACCOUNTTYPE,MACCOUNTTYPE,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public AccountType(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "AccountType";
            _authenticationService = authenticationService;
        }

        public override IEnumerable<MACCOUNTTYPE> GetList(GeneralFilter filter)
        {
            try
            {
                return _context.MACCOUNTTYPE.ToList();
            }
            catch { return new List<MACCOUNTTYPE>(); }
        }


    }
}
