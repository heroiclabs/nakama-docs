[benchmark_ccu_1]: /images/benchmark-ccu-1.png "Benchmark CCU"
[benchmark_ccu_2]: /images/benchmark-ccu-2.png "Benchmark CCU"
[benchmark_ccu_3]: /images/benchmark-ccu-3.png "Benchmark CCU"
[benchmark_login_1]: /images/benchmark-login-1.png "Benchmark Login"
[benchmark_login_2]: /images/benchmark-login-2.png "Benchmark Login"
[benchmark_login_3]: /images/benchmark-login-3.png "Benchmark Login"
[benchmark_login_4]: /images/benchmark-login-4.png "Benchmark Login"
[benchmark_register_1]: /images/benchmark-register-1.png "Benchmark Register"
[benchmark_register_2]: /images/benchmark-register-2.png "Benchmark Register"
[benchmark_register_3]: /images/benchmark-register-3.png "Benchmark Register"
[benchmark_register_4]: /images/benchmark-register-4.png "Benchmark Register"

# Benchmarks

Nakama has been benchmarked under various workload types. Below you'll find parts of the benchmark results.

[Get in touch](mailto:contact@heroiclabs.com?subject=Benchmarks) if you'd like more exhaustive benchmark information.

Performance advantages stem from Nakama's modern hardware-friendly and ultra-scalable architecture. As a result, Nakama's performance grows as the hardware size grows.

Scaling both up and out offers many advantages: from simplified cluster management to access to generally better hardware and economies of scale.

## Benchmark setup

These server workloads demonstrate **single Nakama server** cluster performance with a single server cluster for CockroachDB. The Tsung high-performance benchmark framework is used to run all workloads. All workloads are run with no warmups.

All workloads are conducted on Google Compute Engine.

| Tsung server                          ||
|---             |---                    |
| OS             | Ubuntu 16.04 LTS      |
| Instance Type  | n1-standard-4         |
| CPU            | 4                     |
| Mem            | 15GB                  |

| Nakama server                         ||
|---             |---                    |
| OS             | Ubuntu 16.04 LTS      |
| Version        | Nakama 1.0.1          |
| Instance Type  | n1-standard-1         |
| CPU            | 1                     |
| Mem            | 3.75GB                |

| CockroachDB server                    ||
|---             |---                    |
| OS             | Ubuntu 16.04 LTS      |
| Version        | Nakama 1.0.4          |
| Instance Type  | n1-standard-2         |
| CPU            | 2                     |
| Mem            | 7.5GB                 |

## Workload #1 - Register new user

This workload shows write operations via Nakama to the database server to register a new user.

!!! Summary
    A single Nakama server can handle mean loads of 400 requests/sec with requests served in 6.23 msec (mean) with a database write operation for a new user.

| Registration benchmark results                                                           ||
|---                                                                                       ||
| ![Benchmark Register][benchmark_register_1] | ![Benchmark Register][benchmark_register_2] |
| ![Benchmark Register][benchmark_register_3] | ![Benchmark Register][benchmark_register_4] |

## Workload #2 - Login a user

This workload shows read operations via Nakama to the database server to login a user.

!!! Summary
    A single Nakama server can handle mean loads of 900 requests/sec with requests served in 37.67 msec (mean) with a database read operation for a user.

| Login benchmark results                                                      ||
|---                                                                           ||
| ![Benchmark Login][benchmark_login_1] | ![Benchmark Login][benchmark_login_2] |
| ![Benchmark Login][benchmark_login_3] | ![Benchmark Login][benchmark_login_4] |

## Workload #3 - CCU counts

This workload shows sustained concurrent connected clients which hold a socket open for 100 seconds each.

!!! Summary
    A single Nakama server can handle a peak CCU count of **11,000 users**.

| CCU benchmark results                                                ||
|---                                                                   ||
| ![Benchmark CCU][benchmark_ccu_1] | ![Benchmark CCU][benchmark_ccu_2] |
| ![Benchmark CCU][benchmark_ccu_3]                                    ||





