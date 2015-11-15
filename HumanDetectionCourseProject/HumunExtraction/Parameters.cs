using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanDetection
{
    static class Parameters
    {
        #region Параметры метода MSER и классификатора

        /* Минимальная и максимальная площадь региона относительно
         * общей площади изображения */
        public const Double MinArea = 0.0002;
        public const Double MaxArea = 0.5;

        public const Int32 Delta = 40;

        public const Int32 Dispersiveness = 275;

        #endregion

        #region Параметры лидара

        public const Double FocalLengthEquivalent = 275;
        public const Double ObjectHeight = 2;

        /* #FF0000 is 0 meters, while #0000FF is 100 meters,
         * таким образом, разница между двумя соседними цветами
         * спектра составляет 10,2 cm. */
        public const Double Range = 100;
        public const Int32 LimitRange = 80;

        #endregion
    }
}
