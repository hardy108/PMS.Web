using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PMS.EFCore.Model.Extentions
{

    public static class GoodReceiptExtentions
    {
        public static decimal TotalPrice(this TGR receipt)
        {
            if (receipt.TGRITEM == null)
                return 0;
            decimal TotalPrice = 0;
            receipt.TGRITEM.ToList().ForEach(d => TotalPrice += (d.PRICE *d.QTY ));
            return TotalPrice;
        }

    }

}
