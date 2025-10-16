const axios = require('axios');

/**
 * Một lớp JavaScript để tương tác với HProxy API.
 * * Class này giúp đơn giản hóa việc gọi các endpoint của API bằng cách
 * quản lý base URL, API key và xử lý các phản hồi một cách bất đồng bộ.
 */
class HProxyAPI {
    /**
     * @private
     * @type {import('axios').AxiosInstance}
     */
    #api;
    
    /**
     * @private
     * @type {string}
     */
    #apiKey;

    /**
     * Khởi tạo đối tượng HProxyAPI.
     * @param {string} apiKey - API key của proxy để xác thực với API.
     */
    constructor(apiKey) {
        if (!apiKey) {
            throw new Error("API key là bắt buộc.");
        }
        this.#apiKey = apiKey;

        this.#api = axios.create({
            baseURL: 'https://hproxy.xyz/public/api/v1',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            timeout: 10000 // Timeout sau 10 giây
        });
    }

    /**
     * Một phương thức trợ giúp để thực hiện các yêu cầu GET đến API.
     * @private
     * @param {string} endpoint - Endpoint của API (ví dụ: 'getAllLocations.php').
     * @param {object} [params={}] - Các tham số bổ sung cho yêu cầu.
     * @returns {Promise<any>} - Dữ liệu JSON từ phản hồi của API.
     */
    async #makeRequest(endpoint, params = {}) {
        // Luôn bao gồm api_key trong mỗi yêu cầu
        const queryParams = {
            ...params,
            api_key: this.#apiKey
        };

        try {
            const response = await this.#api.get(endpoint, { params: queryParams });
            return response.data;
        } catch (error) {
            if (error.response) {
                // API trả về lỗi (4xx, 5xx)
                const { status, data } = error.response;
                let errorMessage = `Lỗi HTTP ${status}.`;
                if (status === 400) {
                    errorMessage += " Yêu cầu không hợp lệ. Có thể thiếu tham số.";
                } else if (status === 401) {
                    errorMessage += " API key không hợp lệ hoặc đã hết hạn.";
                }
                console.error(errorMessage, data || '');
            } else if (error.request) {
                // Yêu cầu đã được gửi nhưng không nhận được phản hồi
                console.error("Lỗi mạng: Không nhận được phản hồi từ máy chủ.", error.message);
            } else {
                // Lỗi khác
                console.error("Đã có lỗi xảy ra:", error.message);
            }
            // Ném lại lỗi để có thể bắt ở bên ngoài
            throw error;
        }
    }

    /** Lấy danh sách tất cả các vị trí proxy có sẵn. */
    getAllLocations() {
        return this.#makeRequest('getAllLocations.php');
    }

    /** Lấy danh sách tất cả các routes để tối ưu đường truyền. */
    getRoutes() {
        return this.#makeRequest('getRoutes.php');
    }

    /** Lấy danh sách các loại giao thức proxy (http, https, socks5). */
    getProtoTypes() {
        return this.#makeRequest('getProtoTypes.php');
    }

    /** Lấy thông tin proxy hiện tại đang được sử dụng. */
    getCurrentProxy() {
        return this.#makeRequest('getCurrentProxy.php');
    }

    /**
     * Yêu cầu một proxy mới (xoay proxy).
     * @param {object} [options={}] - Các tùy chọn để lấy proxy.
     * @param {string} [options.countryCode] - Mã quốc gia (mặc định: VN).
     * @param {string} [options.host] - Route (mặc định: route châu Á).
     * @param {string} [options.state] - Tên tiểu bang/khu vực.
     * @param {string} [options.city] - Tên thành phố.
     * @param {string} [options.protoType] - Loại giao thức (mặc định: http).
     * @returns {Promise<any>}
     */
    getNewProxy({ countryCode, host, state, city, protoType } = {}) {
        const params = {};
        if (countryCode) params.country_code = countryCode;
        if (host) params.host = host;
        if (state) params.state = state;
        if (city) params.city = city;
        if (protoType) params.protoType = protoType;

        return this.#makeRequest('getNewProxy.php', params);
    }
}


// =================================================================
//                 VÍ DỤ CÁCH SỬ DỤNG
// =================================================================

// Sử dụng một hàm async IIFE (Immediately Invoked Function Expression) để có thể dùng await
(async () => {
    // Thay 'YOUR_PROXY_API_KEY' bằng API key thực tế của bạn
    const API_KEY = "4168648bac04c35504490Pl";
    const separator = '='.repeat(30);

    try {
        // 1. Khởi tạo đối tượng API
        const proxyClient = new HProxyAPI(API_KEY);

        // 2. Lấy danh sách tất cả các vị trí
        console.log("--- Lấy danh sách vị trí ---");
        const locations = await proxyClient.getAllLocations();
        if (locations?.success) {
            console.log(`Tìm thấy ${locations.locations.length} quốc gia.`);
            // In ra 2 quốc gia đầu tiên làm ví dụ
            locations.locations.slice(0, 2).forEach(country => {
                console.log(`- Quốc gia: ${country.country_name} (${country.region})`);
            });
        }
        console.log(`\n${separator}\n`);

        // 3. Lấy danh sách các routes
        console.log("--- Lấy danh sách routes ---");
        const routes = await proxyClient.getRoutes();
        console.log("Các routes có sẵn:", JSON.stringify(routes, null, 2));
        console.log(`\n${separator}\n`);

        // 4. Lấy danh sách các loại giao thức
        console.log("--- Lấy danh sách loại giao thức ---");
        const protos = await proxyClient.getProtoTypes();
        if (protos?.success) {
            console.log(`Các giao thức hỗ trợ: ${protos.protoTypes.join(', ')}`);
        }
        console.log(`\n${separator}\n`);

        // 5. Lấy proxy mặc định (Việt Nam, HTTP)
        console.log("--- Lấy proxy mới (mặc định) ---");
        const newProxyDefault = await proxyClient.getNewProxy();
        if (newProxyDefault?.success) {
            console.log("Proxy mới (mặc định):");
            console.log(`  - Full Proxy: ${newProxyDefault.proxy.full_proxy}`);
            console.log(`  - Hết hạn sau: ${newProxyDefault.proxy.expires_in_minutes} phút`);
        }
        console.log(`\n${separator}\n`);
        
        // 6. Lấy proxy tùy chỉnh (Ví dụ: Mỹ, SOCKS5)
        console.log("--- Lấy proxy mới (tùy chỉnh: Mỹ, SOCKS5) ---");
        const newProxyCustom = await proxyClient.getNewProxy({ countryCode: 'US', protoType: 'socks5' });
        if (newProxyCustom?.success) {
            console.log("Proxy mới (tùy chỉnh):");
            console.log(`  - Full Proxy: ${newProxyCustom.proxy.full_proxy}`);
            console.log(`  - Giao thức: ${newProxyCustom.proxy.protoType}`);
        }
        console.log(`\n${separator}\n`);

        // 7. Lấy proxy hiện tại
        console.log("--- Lấy proxy hiện tại ---");
        const currentProxy = await proxyClient.getCurrentProxy();
        if (currentProxy?.success) {
            console.log(`Proxy hiện tại là: ${currentProxy.proxy}`);
        }

    } catch (error) {
        console.error("\n>> ĐÃ CÓ LỖI XẢY RA TRONG QUÁ TRÌNH THỰC THI <<");
        // Lỗi chi tiết đã được log bên trong class
    }
})();
