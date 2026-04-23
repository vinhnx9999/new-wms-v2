using Wms.Theme.Web.Util;
using WMSSolution.Shared.MasterData;
namespace Wms.Theme.Web.Model.Reports;

public class VendorMasterDto : VendorMaster
{
    public VendorMasterDto(VendorMaster data)
    {
        Id = data.Id;
        ValidTo = data.ValidTo;
        ContactNumber = data.ContactNumber;
        Company = data.Company;
        ContactName = data.ContactName;
        CreatedDate = data.CreatedDate;
        ValidTo = data.ValidTo;
    }

    public string StrCreatedDate
    {
        get
        {
            return CreatedDate.Convert2LocalTime();
        }
    }

    public string StrValidTo
    {
        get
        {
            return ValidTo.Convert2LocalDate();
        }
    }
}
