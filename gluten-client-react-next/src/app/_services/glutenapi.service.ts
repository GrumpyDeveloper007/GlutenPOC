import axios from 'axios';

class GlutenApiService {

    baseUrl: string;
    httpOptions: any;
    httpOptionsPost: any;

    constructor() {
        this.baseUrl = "https://thegfshire.azurewebsites.net/api";
        this.httpOptions = {}; // Empty headers
        this.httpOptionsPost = {
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json"
            }
        };
    }

    // Error handler
    handleError(error: any, operation = 'operation') {
        console.error(`${operation} failed:`, error); // Log to console
        // Optionally, you could send this error to an external logging service.
        return { error: true, message: error.message || 'An error occurred.' };
    }

    // GET request for PinTopic
    async getPinTopic(country: string) {
        try {
            const response = await axios.get(`${this.baseUrl}/PinTopic?country=${country}`, this.httpOptions);
            return response.data;
        } catch (error) {
            return this.handleError(error, `getPinTopic country=${country}`);
        }
    }

    // GET request for GMapsPin
    async getGMPin(country: string) {
        try {
            const response = await axios.get(`${this.baseUrl}/GMapsPin?country=${country}`, this.httpOptions);
            return response.data;
        } catch (error) {
            return this.handleError(error, `getGMPin country=${country}`);
        }
    }

    // POST request for MapHome
    async postMapHome(geoLatitude: number, geoLongitude: number) {
        console.debug("postMapHome:", geoLatitude, geoLongitude);
        try {
            const response = await axios.post(
                `${this.baseUrl}/MapHome`,
                { geoLatitude, geoLongitude },
                this.httpOptionsPost
            );
            return response.data;
        } catch (error) {
            return this.handleError(error, 'postMapHome');
        }
    }
}

export default new GlutenApiService();
