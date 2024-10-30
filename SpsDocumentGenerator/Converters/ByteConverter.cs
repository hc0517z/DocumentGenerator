using System.Globalization;

namespace SpsDocumentGenerator.Converters
{
    public abstract class ByteConverter
    {
        public static string FormatBytes(long byteCount)
        {
            string[] suffixes = { "Byte", "KB", "MB", "GB", "TB", "PB", "EB" };
            int i = 0;
            double size = byteCount;

            while (size >= 1024 && i < suffixes.Length - 1)
            {
                size /= 1024;
                i++;
            }

            // 소수점 이하를 표시할지 여부 결정
            string format = i == 0 ? "N0" : "N2"; // bytes는 소수점 없음, 나머지는 소수점 2자리

            // 형식화된 문자열 생성
            string formattedString = size.ToString(format, CultureInfo.InvariantCulture) + " " + suffixes[i];

            return formattedString;
        }
    }
}