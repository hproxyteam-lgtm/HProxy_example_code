using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HProxyApiClient; // Sử dụng namespace đã định nghĩa ở trên

public class Program
{
    public static async Task Main(string[] args)
    {
        // Thay 'YOUR_PROXY_API_KEY' bằng API key thực tế của bạn
        const string ApiKey = "4168648bac04c35504490Pl";

        try
        {
            // 1. Khởi tạo đối tượng API
            var proxyClient = new HProxyAPI(ApiKey);
            var separator = new string('=', 30);

            // 2. Lấy danh sách tất cả các vị trí
            Console.WriteLine("--- Lấy danh sách vị trí ---");
            var locations = await proxyClient.GetAllLocationsAsync();
            if (locations?.Success == true)
            {
                Console.WriteLine($"Tìm thấy {locations.Locations.Count} quốc gia.");
                // In ra 2 quốc gia đầu tiên làm ví dụ
                foreach (var country in locations.Locations.Take(2))
                {
                    Console.WriteLine($"- Quốc gia: {country.CountryName} ({country.Region})");
                }
            }
            Console.WriteLine($"\n{separator}\n");

            // 3. Lấy danh sách các routes
            Console.WriteLine("--- Lấy danh sách routes ---");
            var routes = await proxyClient.GetRoutesAsync();
            Console.WriteLine($"Các routes có sẵn: {JsonSerializer.Serialize(routes, new JsonSerializerOptions { WriteIndented = true })}");
            Console.WriteLine($"\n{separator}\n");

            // 4. Lấy danh sách các loại giao thức
            Console.WriteLine("--- Lấy danh sách loại giao thức ---");
            var protos = await proxyClient.GetProtoTypesAsync();
            if (protos?.Success == true)
            {
                Console.WriteLine($"Các giao thức hỗ trợ: {string.Join(", ", protos.ProtoTypes)}");
            }
            Console.WriteLine($"\n{separator}\n");

            // 5. Lấy proxy mặc định (Việt Nam, HTTP)
            Console.WriteLine("--- Lấy proxy mới (mặc định) ---");
            var newProxyDefault = await proxyClient.GetNewProxyAsync();
            if (newProxyDefault?.Success == true)
            {
                Console.WriteLine("Proxy mới (mặc định):");
                Console.WriteLine($"  - Full Proxy: {newProxyDefault.Proxy.FullProxy}");
                Console.WriteLine($"  - Hết hạn sau: {newProxyDefault.Proxy.ExpiresInMinutes} phút");
            }
            Console.WriteLine($"\n{separator}\n");

            // 6. Lấy proxy tùy chỉnh (Ví dụ: Mỹ, SOCKS5)
            Console.WriteLine("--- Lấy proxy mới (tùy chỉnh: Mỹ, SOCKS5) ---");
            var newProxyCustom = await proxyClient.GetNewProxyAsync(countryCode: "US", protoType: "socks5");
            if (newProxyCustom?.Success == true)
            {
                Console.WriteLine("Proxy mới (tùy chỉnh):");
                Console.WriteLine($"  - Full Proxy: {newProxyCustom.Proxy.FullProxy}");
                Console.WriteLine($"  - Giao thức: {newProxyCustom.Proxy.ProtoType}");
            }
            Console.WriteLine($"\n{separator}\n");

            // 7. Lấy proxy hiện tại
            Console.WriteLine("--- Lấy proxy hiện tại ---");
            var currentProxy = await proxyClient.GetCurrentProxyAsync();
            if (currentProxy?.Success == true)
            {
                Console.WriteLine($"Proxy hiện tại là: {currentProxy.Proxy}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nĐÃ XẢY RA LỖI: {ex.Message}");
        }
    }
}
