using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Shared.Models;


namespace PMS.Web.Services.Controllers.General
{
    
    [ApiController]
    public class ReferenceController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "reference"; //Replace with your own route name

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/religion")]
        public IEnumerable<SelectOptionItem> GetReligionForSelect()
        {
            return (new List<string> { "Islam", "Kristen", "Katolik", "Hindu", "Budha", "Konghucu", "Lain-lain" }).Select(c => new SelectOptionItem() { Id = c, Text = c });
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/gender")]
        public IEnumerable<SelectOptionItem> GetGenderForSelect()
        {
            return new List<SelectOptionItem>
            {
                new SelectOptionItem{Id="L",Text="Laki-laki"},
                new SelectOptionItem{Id="P",Text="Perempuan"}
            };
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/transactionstatus")]
        public IEnumerable<SelectOptionItem> GetTransactionStatusForSelect()
        {
            return new List<SelectOptionItem>
            {
                new SelectOptionItem{Id="",Text="None"},
                new SelectOptionItem{Id="A",Text="Approved"},
                new SelectOptionItem{Id="C",Text="Canceled"},
                new SelectOptionItem{Id="P",Text="Processed"},
                new SelectOptionItem{Id="D",Text="Deleted"},
            };
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/education")]
        public IEnumerable<SelectOptionItem> GetEducationForSelect()
        {
            return (new List<string> { "-", "SD","SMP","SMA","SMK","D1","D2","D3","S1","S2","S3","Lain-lain" }).Select(c => new SelectOptionItem() { Id = c, Text = c });
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/familyrelation")]
        public IEnumerable<SelectOptionItem> GetFamilyRelation()
        {
            return (new List<string> { "Suami", "Istri", "Anak","Orang Tua","Lain-lain" }).Select(c => new SelectOptionItem() { Id = c, Text = c });
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/filedoctype")]
        public IEnumerable<SelectOptionItem> GetFileDocumentType()
        {
            return new List<SelectOptionItem>
            {
                new SelectOptionItem{Id="KTP",Text="KTP"},
                new SelectOptionItem{Id="KK",Text="Kartu Keluarga"},
                new SelectOptionItem{Id="NPWP",Text="NPWP"},
                new SelectOptionItem{Id="BPJS-TK",Text="BPJS Ketenagakerjaan"},
                new SelectOptionItem{Id="BPJS-KS",Text="BPJS Kesehatan"},
                new SelectOptionItem{Id="SIM",Text="SIM"},
                new SelectOptionItem{Id="OTHER",Text="Lain-lain"}

            };
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/harvesttype")]
        public IEnumerable<SelectOptionItem> GetHarvestType()
        {
            return new List<SelectOptionItem>
            {
                new SelectOptionItem{Id="0",Text="Potong Buah"},
                new SelectOptionItem{Id="1",Text="Kutip Brondol"}
            };
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/paymenttype")]
        public IEnumerable<SelectOptionItem> GetPaymentType()
        {
            return new List<SelectOptionItem>
            {
                new SelectOptionItem{Id="0",Text="Harian"},
                new SelectOptionItem{Id="1",Text="Kontanan"},
                new SelectOptionItem{Id="2",Text="Borongan"},
                
            };
        }


        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/rfcardtype")]
        public IEnumerable<SelectOptionItem> GetRFCardType()
        {
            return new List<SelectOptionItem>
            {
                new SelectOptionItem{Id="",Text="Tanpa Kartu"},
                new SelectOptionItem{Id="GA",Text="GA"},
                new SelectOptionItem{Id="HVT",Text="Panen"},
                new SelectOptionItem{Id="UPK",Text="Perawatan"}

            };
        }

    }
}