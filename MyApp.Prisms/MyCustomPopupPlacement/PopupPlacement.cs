using System.Windows;
using System.Windows.Controls.Primitives;

namespace MyApp.Prisms.MyCustomPopupPlacement
{
    public class PopupPlacement
    {
        public static CustomPopupPlacementCallback BottomHorizontalCenter { get; set; } =
            new CustomPopupPlacementCallback((Size popupSize, Size targetSize, Point offset) =>
            {
                var horOffset = (targetSize.Width - popupSize.Width) / 2;

                CustomPopupPlacement placement1 =
                    new CustomPopupPlacement(new Point(horOffset, targetSize.Height), PopupPrimaryAxis.Vertical);

                var hverOffset = (targetSize.Height - popupSize.Height) / 2;

                CustomPopupPlacement placement2 =
                    new CustomPopupPlacement(new Point(targetSize.Width, hverOffset), PopupPrimaryAxis.Horizontal);

                CustomPopupPlacement[] ttplaces =
                    new CustomPopupPlacement[] { placement1, placement2 };
                return ttplaces;
            });
    }
}