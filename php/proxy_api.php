<?php

class HProxyAPI
{
    /**
     * URL cơ sở cho tất cả các yêu cầu API.
     * @var string
     */
    private const BASE_URL = "https://hproxy.xyz/public/api/v1";

    /**
     * API key của proxy.
     * @var string
     */
    private $apiKey;

    /**
     * Khởi tạo đối tượng HProxyAPI.
     *
     * @param string $apiKey API key của proxy để xác thực.
     * @throws Exception Nếu api_key không được cung cấp.
     */
    public function __construct(string $apiKey)
    {
        if (empty($apiKey)) {
            throw new Exception("API key là bắt buộc.");
        }
        $this->apiKey = $apiKey;
    }

    /**
     * Một phương thức trợ giúp để thực hiện các yêu cầu GET đến API.
     *
     * @param string $endpoint Endpoint của API (ví dụ: 'getAllLocations.php').
     * @param array $params Các tham số bổ sung cho yêu cầu.
     * @return array Dữ liệu JSON từ phản hồi của API dưới dạng mảng.
     * @throws Exception Nếu có lỗi xảy ra trong quá trình gọi API.
     */
    private function _make_request(string $endpoint, array $params = []): array
    {
        // Luôn bao gồm api_key trong mỗi yêu cầu
        $queryParams = array_merge(['api_key' => $this->apiKey], $params);
        $url = self::BASE_URL . "/" . $endpoint . "?" . http_build_query($queryParams);

        $ch = curl_init();

        curl_setopt($ch, CURLOPT_URL, $url);
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt($ch, CURLOPT_HTTPHEADER, [
            'Content-Type: application/json',
            'Accept: application/json'
        ]);

        $response = curl_exec($ch);
        $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
        $error = curl_error($ch);

        curl_close($ch);

        if ($error) {
            throw new Exception("Lỗi cURL: " . $error);
        }

        if ($httpCode >= 400) {
            $errorMessage = "Lỗi HTTP " . $httpCode;
            if ($httpCode == 400) {
                $errorMessage .= ": Yêu cầu không hợp lệ. Có thể thiếu tham số.";
            } elseif ($httpCode == 401) {
                $errorMessage .= ": API key không hợp lệ hoặc đã hết hạn.";
            }
            throw new Exception($errorMessage . " - Phản hồi: " . $response);
        }

        $decodedResponse = json_decode($response, true);
        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new Exception("Không thể giải mã phản hồi JSON.");
        }

        return $decodedResponse;
    }

    /**
     * Lấy danh sách tất cả các vị trí proxy có sẵn.
     * @return array
     */
    public function getAllLocations(): array
    {
        return $this->_make_request("getAllLocations.php");
    }

    /**
     * Lấy danh sách tất cả các routes để tối ưu đường truyền.
     * @return array
     */
    public function getRoutes(): array
    {
        return $this->_make_request("getRoutes.php");
    }

    /**
     * Lấy danh sách các loại giao thức proxy (http, https, socks5).
     * @return array
     */
    public function getProtoTypes(): array
    {
        return $this->_make_request("getProtoTypes.php");
    }

    /**
     * Lấy thông tin proxy hiện tại đang được sử dụng.
     * @return array
     */
    public function getCurrentProxy(): array
    {
        return $this->_make_request("getCurrentProxy.php");
    }

    /**
     * Yêu cầu một proxy mới (xoay proxy).
     *
     * @param string|null $countryCode Mã quốc gia (mặc định: VN).
     * @param string|null $host Route (mặc định: route châu Á).
     * @param string|null $state Tên tiểu bang/khu vực.
     * @param string|null $city Tên thành phố.
     * @param string|null $protoType Loại giao thức (mặc định: http).
     * @return array Thông tin chi tiết về proxy mới.
     */
    public function getNewProxy(
        ?string $countryCode = null,
        ?string $host = null,
        ?string $state = null,
        ?string $city = null,
        ?string $protoType = null
    ): array {
        $params = [];
        // Chỉ thêm các tham số vào yêu cầu nếu chúng được cung cấp
        if ($countryCode !== null) $params['country_code'] = $countryCode;
        if ($host !== null) $params['host'] = $host;
        if ($state !== null) $params['state'] = $state;
        if ($city !== null) $params['city'] = $city;
        if ($protoType !== null) $params['protoType'] = $protoType;

        return $this->_make_request("getNewProxy.php", $params);
    }
}

// =================================================================
//                 VÍ DỤ CÁCH SỬ DỤNG
// =================================================================

// Thay 'YOUR_PROXY_API_KEY' bằng API key thực tế của bạn
$apiKey = "4168648bac04c35504490Pl";

try {
    // 1. Khởi tạo đối tượng API
    $proxyClient = new HProxyAPI($apiKey);

    // 2. Lấy danh sách tất cả các vị trí
    echo "--- Lấy danh sách vị trí ---\n";
    $locations = $proxyClient->getAllLocations();
    if ($locations['success'] ?? false) {
        $countries = $locations['locations'] ?? [];
        echo "Tìm thấy " . count($countries) . " quốc gia.\n";
        // In ra 2 quốc gia đầu tiên làm ví dụ
        foreach (array_slice($countries, 0, 2) as $country) {
            echo "- Quốc gia: " . $country['country_name'] . " (" . $country['region'] . ")\n";
        }
    }
    echo "\n" . str_repeat("=", 30) . "\n\n";

    // 3. Lấy danh sách các routes
    echo "--- Lấy danh sách routes ---\n";
    $routes = $proxyClient->getRoutes();
    echo "Các routes có sẵn: " . json_encode($routes, JSON_PRETTY_PRINT) . "\n";
    echo "\n" . str_repeat("=", 30) . "\n\n";

    // 4. Lấy danh sách các loại giao thức
    echo "--- Lấy danh sách loại giao thức ---\n";
    $protos = $proxyClient->getProtoTypes();
    if ($protos['success'] ?? false) {
        echo "Các giao thức hỗ trợ: " . implode(', ', $protos['protoTypes'] ?? []) . "\n";
    }
    echo "\n" . str_repeat("=", 30) . "\n\n";

    // 5. Lấy proxy mặc định (Việt Nam, HTTP)
    echo "--- Lấy proxy mới (mặc định) ---\n";
    $newProxyDefault = $proxyClient->getNewProxy();
    if ($newProxyDefault['success'] ?? false) {
        echo "Proxy mới (mặc định):\n";
        echo "  - Full Proxy: " . ($newProxyDefault['proxy']['full_proxy'] ?? 'N/A') . "\n";
        echo "  - Hết hạn sau: " . ($newProxyDefault['proxy']['expires_in_minutes'] ?? 'N/A') . " phút\n";
    }
    echo "\n" . str_repeat("=", 30) . "\n\n";

    // 6. Lấy proxy tùy chỉnh (Ví dụ: Mỹ, SOCKS5)
    echo "--- Lấy proxy mới (tùy chỉnh: Mỹ, SOCKS5) ---\n";
    $newProxyCustom = $proxyClient->getNewProxy(countryCode: 'US', protoType: 'socks5');
    if ($newProxyCustom['success'] ?? false) {
        echo "Proxy mới (tùy chỉnh):\n";
        echo "  - Full Proxy: " . ($newProxyCustom['proxy']['full_proxy'] ?? 'N/A') . "\n";
        echo "  - Giao thức: " . ($newProxyCustom['proxy']['protoType'] ?? 'N/A') . "\n";
    }
    echo "\n" . str_repeat("=", 30) . "\n\n";

    // 7. Lấy proxy hiện tại
    echo "--- Lấy proxy hiện tại ---\n";
    $currentProxy = $proxyClient->getCurrentProxy();
    if ($currentProxy['success'] ?? false) {
        echo "Proxy hiện tại là: " . ($currentProxy['proxy'] ?? 'N/A') . "\n";
    }
} catch (Exception $e) {
    echo "Đã xảy ra lỗi: " . $e->getMessage() . "\n";
}

?>
