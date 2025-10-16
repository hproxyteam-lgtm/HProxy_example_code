import requests

class HProxyAPI:
    """
    Một lớp Python để tương tác với HProxy API.

    Class này giúp đơn giản hóa việc gọi các endpoint của API bằng cách
    quản lý base URL, API key và xử lý các phản hồi.

    Attributes:
        api_key (str): API key của proxy (không phải của người dùng).
        base_url (str): URL cơ sở cho tất cả các yêu cầu API.
        session (requests.Session): Một session để tái sử dụng kết nối.
    """
    
    BASE_URL = "https://hproxy.xyz/public/api/v1"

    def __init__(self, api_key: str):
        """
        Khởi tạo đối tượng HProxyAPI.

        Args:
            api_key (str): API key của proxy để xác thực với API.
        
        Raises:
            ValueError: Nếu api_key không được cung cấp.
        """
        if not api_key:
            raise ValueError("API key là bắt buộc.")
            
        self.api_key = api_key
        self.session = requests.Session()
        self.session.headers.update({
            "Content-Type": "application/json",
            "Accept": "application/json"
        })

    def _make_request(self, endpoint: str, params: dict = None) -> dict:
        """
        Một phương thức trợ giúp để thực hiện các yêu cầu GET đến API.

        Args:
            endpoint (str): Endpoint của API (ví dụ: 'getAllLocations.php').
            params (dict, optional): Các tham số bổ sung cho yêu cầu.

        Returns:
            dict: Dữ liệu JSON từ phản hồi của API.

        Raises:
            requests.exceptions.HTTPError: Nếu API trả về một mã lỗi HTTP.
            requests.exceptions.RequestException: Cho các lỗi mạng khác.
        """
        url = f"{self.BASE_URL}/{endpoint}"
        
        # Luôn luôn bao gồm api_key trong mỗi yêu cầu
        base_params = {"api_key": self.api_key}
        if params:
            base_params.update(params)
        
        try:
            response = self.session.get(url, params=base_params)
            # Ném ra một ngoại lệ cho các mã trạng thái lỗi (4xx hoặc 5xx)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.HTTPError as http_err:
            # Xử lý các lỗi cụ thể từ API
            if response.status_code == 400:
                print(f"Lỗi 400: Yêu cầu không hợp lệ. Có thể thiếu tham số.")
            elif response.status_code == 401:
                print(f"Lỗi 401: API key không hợp lệ hoặc đã hết hạn.")
            else:
                print(f"Lỗi HTTP xảy ra: {http_err} - {response.text}")
            raise
        except requests.exceptions.RequestException as req_err:
            print(f"Lỗi kết nối đến API: {req_err}")
            raise

    def get_all_locations(self) -> dict:
        """Lấy danh sách tất cả các vị trí proxy có sẵn."""
        return self._make_request("getAllLocations.php")

    def get_routes(self) -> dict:
        """Lấy danh sách tất cả các routes để tối ưu đường truyền."""
        return self._make_request("getRoutes.php")

    def get_proto_types(self) -> dict:
        """Lấy danh sách các loại giao thức proxy (http, https, socks5)."""
        return self._make_request("getProtoTypes.php")

    def get_current_proxy(self) -> dict:
        """Lấy thông tin proxy hiện tại đang được sử dụng."""
        return self._make_request("getCurrentProxy.php")

    def get_new_proxy(self, country_code: str = None, host: str = None, 
                      state: str = None, city: str = None, protoType: str = None) -> dict:
        """
        Yêu cầu một proxy mới (xoay proxy).
        Tất cả các tham số đều là tùy chọn.

        Args:
            country_code (str, optional): Mã quốc gia (mặc định: VN).
            host (str, optional): Route (mặc định: route châu Á).
            state (str, optional): Tên tiểu bang/khu vực.
            city (str, optional): Tên thành phố.
            protoType (str, optional): Loại giao thức (mặc định: http).

        Returns:
            dict: Thông tin chi tiết về proxy mới.
        """
        params = {}
        # Chỉ thêm các tham số vào yêu cầu nếu chúng được cung cấp
        if country_code:
            params["country_code"] = country_code
        if host:
            params["host"] = host
        if state:
            params["state"] = state
        if city:
            params["city"] = city
        if protoType:
            params["protoType"] = protoType
            
        return self._make_request("getNewProxy.php", params=params)
    
if __name__ == "__main__":
    # Thay 'YOUR_PROXY_API_KEY' bằng API key thực tế của bạn
    API_KEY = "4168648bac04c35504490Pl"

    try:
        # 1. Khởi tạo đối tượng API
        proxy_client = HProxyAPI(api_key=API_KEY)

        # 2. Lấy danh sách tất cả các vị trí
        print("--- Lấy danh sách vị trí ---")
        locations = proxy_client.get_all_locations()
        if locations.get("success"):
            # In ra 2 quốc gia đầu tiên làm ví dụ
            print(f"Tìm thấy {len(locations.get('locations', []))} quốc gia.")
            for country in locations.get('locations', [])[:2]:
                print(f"- Quốc gia: {country['country_name']} ({country['region']})")
        print("\n" + "="*30 + "\n")

        # 3. Lấy danh sách các routes
        print("--- Lấy danh sách routes ---")
        routes = proxy_client.get_routes()
        print(f"Các routes có sẵn: {routes}")
        print("\n" + "="*30 + "\n")
        
        # 4. Lấy danh sách các loại giao thức
        print("--- Lấy danh sách loại giao thức ---")
        protos = proxy_client.get_proto_types()
        if protos.get("success"):
            print(f"Các giao thức hỗ trợ: {protos.get('protoTypes')}")
        print("\n" + "="*30 + "\n")

        # 5. Lấy proxy mặc định (Việt Nam, HTTP)
        print("--- Lấy proxy mới (mặc định) ---")
        new_proxy_default = proxy_client.get_new_proxy()
        if new_proxy_default.get("success"):
            print("Proxy mới (mặc định):")
            print(f"  - Full Proxy: {new_proxy_default['proxy']['full_proxy']}")
            print(f"  - Hết hạn sau: {new_proxy_default['proxy']['expires_in_minutes']} phút")
        print("\n" + "="*30 + "\n")
        
        # 6. Lấy proxy tùy chỉnh (Ví dụ: Mỹ, SOCKS5)
        print("--- Lấy proxy mới (tùy chỉnh: Mỹ, SOCKS5) ---")
        new_proxy_custom = proxy_client.get_new_proxy(country_code="US", protoType="socks5")
        if new_proxy_custom.get("success"):
            print("Proxy mới (tùy chỉnh):")
            print(f"  - Full Proxy: {new_proxy_custom['proxy']['full_proxy']}")
            print(f"  - Giao thức: {new_proxy_custom['proxy']['protoType']}")
        print("\n" + "="*30 + "\n")
        
        # 7. Lấy proxy hiện tại
        print("--- Lấy proxy hiện tại ---")
        current_proxy = proxy_client.get_current_proxy()
        if current_proxy.get("success"):
            print(f"Proxy hiện tại là: {current_proxy.get('proxy')}")

    except ValueError as ve:
        print(f"Lỗi khởi tạo: {ve}")
    except requests.exceptions.HTTPError:
        # Các thông báo lỗi chi tiết đã được in trong phương thức _make_request
        print("Đã xảy ra lỗi HTTP. Vui lòng kiểm tra lại API key hoặc các tham số.")
    except requests.exceptions.RequestException as e:
        print(f"Không thể kết nối đến máy chủ API: {e}")
    except Exception as e:
        print(f"Đã có lỗi không mong muốn xảy ra: {e}")
