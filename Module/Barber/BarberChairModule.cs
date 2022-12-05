

namespace VMP_CNR.Module.Barber
{
    public class BarberChairModule : SqlModule<BarberChairModule, BarberChair, uint>
    {

        protected override string GetQuery()
        {
            return "SELECT * FROM `barber_shop_chairs`;";
        }
    }
}
