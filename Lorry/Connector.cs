using System;
using System.Collections.Generic;
using ServiceStack;
using Lorry.Dto;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Lorry
{
    // общение с сервером
    public class Connector
    {        
        public string AuthServer { get; set; }

        public string Token { get; set; }
        public string TokenType { get; set; }
        public string Server { get; set; }


        /// <summary>
        /// Пустое обращение к сервру, чтобы ServiceStack смирлся с протухшим сертификатом
        /// </summary>
        internal void SllOff()
        {
            Task.Run(() =>
            {
                var url = AuthServer + $"/mobile/password/create";
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                    var client = new JsonServiceClient();
                    var response = client.Get<string>(url);
                }
                catch (Exception e)
                {
                    return;
                }
            });
        }


        public Task<bool> Login(string phone, string password)
        {
            return Task.Run(() =>
            {
                var url = AuthServer + "/mobile/core/login-driver";
                var request = new LoginRequest
                {
                    Phone = phone,
                    Password = password,
                    Token = "0000",
                    NotificationToken = "PKCHz4QRpC_h_XkT2btmm:APA91bFz5fVPk_avPpoDW6WhrVhamV67XLnnX-d3BzsSoI0hUmCBvxLXjLEinvkrS8qocQonyH7szigP6J1la3wBa8yczfoRj4b4eiqCQYiuq6JI616ceSMc8H9by737V72xFSlrpfrS"
                };
                var json = JsonConvert.SerializeObject(request);
                try
                {
                    var result = url.PostJsonToUrl(json);
                    var response = JsonConvert.DeserializeObject<LoginResponse>(result);
                    Server = response.Server;
                    Token = response.Token;
                    TokenType = response.TokenType;
                    return true;
                }
                catch
                {                   
                    return false;
                }
            });
        }  
        
        public Task<Trip> LoadTrip()
        {
            return Task.Run(() =>
            {
                try
                {
                    var url = Server + "/mobile/users/trip";
                    var result = url.GetJsonFromUrl(requestFilter: webReq =>
                    {
                        webReq.Headers["authorization"] = TokenType + " " + Token;
                    });
                    if (result.Contains("\"timeslot\":[]")) // нет рейса
                    {
                        return new Trip { timeslot = new Timeslot { status_id = 0 } };
                    }
                    var response = JsonConvert.DeserializeObject<Trip>(result);
                    return response;
                }catch
                {
                    return null;
                }
            });
        }

        internal Task<bool> SetLoad(int id, int driverId, string time)
        {
            return Task.Run(() =>
            {
                var url = Server + "/mobile/trips/logs";
                var log = new LogTrip
                {
                    logs = new List<Log>
                {
                    new Log
                    {
                        arrivalTime = time,
                        responseTime = time,
                        loggingTime = time,
                        loadingTime = time,
                        status = "loaded",
                        id = id,
                        isLoadingAccepted = 1,
                        isLoadingSuccess = 1,
                        isReload = false,
                        isStatusReversed = 0,
                        isWrongField = 0
                    }
                },
                    loadedTrips = new List<object>(),
                    driverId = driverId,
                    time = time,
                    version = 1,
                    cache = DateTime.Now.Ticks
                };
                var json = JsonConvert.SerializeObject(log);
                try
                {
                    var result = url.PostJsonToUrl(json, webReq =>
                    {
                        webReq.Headers["authorization"] = TokenType + " " + Token;
                    });
                    var response = JsonConvert.DeserializeObject<Trip>(result);
                    return true;
                }catch
                {
                    return false;
                }

            });
        }

        internal Task<int> NextTrip(bool value, int trip)
        {
            return Task.Run(() => 
            {
                var url = Server + $"/mobile/trips/{trip}/confirm-finish";
                var json = JsonConvert.SerializeObject(new Confirm { Ready = value });
                try
                {
                    var result = url.PostJsonToUrl(json, webReq =>
                    {
                        webReq.Headers["authorization"] = TokenType + " " + Token;
                    });
                    return 200;
                }catch(Exception e)
                {
                    if (e.Message.Contains("401"))
                    {
                        return 401;
                    }
                    return 0;
                }
            });
        }

        internal Task<DriverProfile> LoadProfile()
        {
            return Task.Run(() =>
            {
                try
                {
                    var url = Server + "/mobile/users/current";
                    var result = url.GetJsonFromUrl(requestFilter: webReq =>
                    {
                        webReq.Headers["authorization"] = TokenType + " " + Token;
                    });
                    var response = JsonConvert.DeserializeObject<DriverProfile>(result);
                    return response;
                }
                catch(Exception e)
                {                   
                    if (e.Message.Contains("401"))
                    {
                        return new DriverProfile { driver = new Driver { id = 0 } };
                    }
                    return null;
                }
            });
        }

        internal Task<int> GetPassword(string phone)
        {
            return Task.Run(() =>
            {
                var url = AuthServer + $"/mobile/password/create?phone={phone}";                
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                    var client = new JsonServiceClient();
                    var response = client.Get<string>(url);

                    //var result = url.GetJsonFromUrl();
                    return 200;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Authentication failed"))
                    {
                        return 401;
                    }
                    return 0;
                }
            });
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
        {
            //Do something to check the certificate is valid. 
            return true;
        }

        /// <summary>
        /// Выход из режима неактивности
        /// </summary>
        /// <param name="activCarId"></param>
        /// <returns></returns>
        internal Task<int> RequestJob(int activCarId)
        {
            return Task.Run(() =>
            {
                var url = Server + "/mobile/users/ready-to-trip";
                var json = JsonConvert.SerializeObject(new TripRequest { Car = activCarId });
                try
                {
                    var result = url.PostJsonToUrl(json, webReq =>
                    {
                        webReq.Headers["authorization"] = TokenType + " " + Token;
                    });
                    return 200;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("401"))
                    {
                        return 401;
                    }
                    return 0;
                }
            });
        }
    }
}