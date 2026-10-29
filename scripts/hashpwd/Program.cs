var hash = args.Length > 0 ? args[0] : "$2a$11$QZ5xo2WiMjPYwsa4MyDTSuyhU0jFd7VQbgk4w5iKe/xpL1pHZajt.";
var ok = BCrypt.Net.BCrypt.Verify("Allah786@", hash);
Console.WriteLine(ok ? "VERIFY_OK" : "VERIFY_FAIL");
