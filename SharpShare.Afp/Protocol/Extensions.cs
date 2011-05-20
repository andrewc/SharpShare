using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol {
    public static class Extensions {
        public static IEnumerable<TEnum> EnumerateFlags<TEnum>(this TEnum enumEnum) where TEnum : struct {
            long value = Convert.ToInt64(enumEnum);
            long current = 1;

            while (current != 0) {
                long thisValue = (value & current);
                current <<= 1;

                object enumValue = Enum.ToObject(typeof(TEnum), thisValue);

                if (System.Enum.IsDefined(typeof(TEnum), enumValue)) {
                    yield return (TEnum)enumValue;
                }
            }
        }

        public static uint ToMacintoshDate(this DateTime date) {
            date = date.ToUniversalTime();
            DateTime baseTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return (uint)(date - baseTime).TotalSeconds;
        }
    }
}
