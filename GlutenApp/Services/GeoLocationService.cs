using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlutenApp.Services
{
    internal class GeoLocationService
    {
        public async Task<Placemark> GetLocationCountryCode()
        {
            try
            {
                Location location = await Geolocation.Default.GetLastKnownLocationAsync();

                if (location != null)
                {
                    IEnumerable<Placemark> placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);

                    Placemark placemark = placemarks?.FirstOrDefault();

                    if (placemark != null)
                    {
                        //return
                        //    $"AdminArea:       {placemark.AdminArea}\n" +
                        //    $"CountryCode:     {placemark.CountryCode}\n" +
                        //    $"CountryName:     {placemark.CountryName}\n" +
                        //    $"FeatureName:     {placemark.FeatureName}\n" +
                        //    $"Locality:        {placemark.Locality}\n" +
                        //    $"PostalCode:      {placemark.PostalCode}\n" +
                        //    $"SubAdminArea:    {placemark.SubAdminArea}\n" +
                        //    $"SubLocality:     {placemark.SubLocality}\n" +
                        //    $"SubThoroughfare: {placemark.SubThoroughfare}\n" +
                        //    $"Thoroughfare:    {placemark.Thoroughfare}\n";
                        return placemark;
                    }

                }
            }
            catch (FeatureNotSupportedException)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException)
            {
                // Handle permission exception
            }
            catch (Exception)
            {
                // Unable to get location
            }

            return null;
        }
    }
}
