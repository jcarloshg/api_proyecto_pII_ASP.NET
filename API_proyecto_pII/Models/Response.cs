using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_proyecto_pII.Models
{
    internal class RespuestaSetUser
    {
        public string Code { get; set; }   
        public string Message { get; set; }
        public string Data { get; set; }
        public string Status { get; set; }
    }

    internal class RespuestaUpdateUser
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
        public string Status { get; set; }
    }

    internal class RespuestaGetUsers
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string[] Data { get; set; }
        public string Status { get; set; }
    }

    internal class RespuestaGetUsersInfo
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public Dictionary<string, UserInfo> Data { get; set; }
        public string Status { get; set; }
    }

    internal class RespuestaSetUserInfo
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
        public string Status { get; set; }
    }

    internal class RespuestaUpdateUserInfo
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
        public string Status { get; set; }
    }

    internal class RespuestaLogin
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public UserInfo Data { get; set; }
        public string Status { get; set; }
    }

    internal class ResPass
    {
        public string pass { get; set; }
    }
}