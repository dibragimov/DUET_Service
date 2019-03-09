using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUETCPMConsole
{
    public class IntegerConstants
    {
        public static int SUCCESS = 7; //= успешно, "-1" 
        public static int ERROR = 1; //= ошибка, 
        public static int WRONG_PARAMS = 400;//не указаны необходимые параметры запроса, 
        public static int DUPLICATE_SESSION_ID = 404;//= такой sessionId уже есть, 
        public static int WRONG_CHECKSUM = 500; ///"-500" = Не верная контрольная сумма
        public static int WRONG_TIME = 403; ///"-403" = Не верное время
    }
}
