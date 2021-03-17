#!/usr/bin/env pwsh

# The list of benchmarks to run
$BENCHMARKS_TO_RUN=$ARGS
#  ...or use all the *_bench dirs by default
if ([string]::IsNullOrEmpty($BENCHMARKS_TO_RUN)) {
    $BENCHMARKS_TO_RUN=$((Get-ChildItem -Directory -Path '*_bench').Name | Sort-Object)
}

$BENCH_NAME=$(cat ./bench_command)
$RESULTS_DIR="results/$([DateTime]::Now.ToString('yyyyddMMTHHmmss'))"
$GRPC_BENCHMARK_DURATION="30s"
$GRPC_SERVER_CPUS="1"
$GRPC_SERVER_RAM="512m"
$GRPC_CLIENT_CONNECTIONS="5"
$GRPC_CLIENT_CONCURRENCY="50"
#$GRPC_CLIENT_QPS="0"
#$GRPC_CLIENT_QPS=$($GRPC_CLIENT_QPS / $GRPC_CLIENT_CONCURRENCY)
$GRPC_CLIENT_CPUS="1"
#$GRPC_REQUEST_PAYLOAD="100B"

$CWD = $PWD.Path

foreach ($benchmark in ${BENCHMARKS_TO_RUN}) {
	$NAME=$benchmark
	echo "==> Running benchmark for ${NAME}..."

	mkdir -Path "${RESULTS_DIR}" -Force > $null
	docker run --name "${NAME}" --rm `
		--cpus "${GRPC_SERVER_CPUS}" `
		--memory "${GRPC_SERVER_RAM}" `
		-e GRPC_SERVER_CPUS `
        -e ASPNETCORE_ENVIRONMENT=Development `
		--network=host --detach --tty "${NAME}" > $null
	Start-Sleep -Second 5
	$job = Start-Job {. ${using:CWD}/collect_stats.ps1 -NAME "${using:NAME}" -REPORT_DIR "${using:CWD}/${using:RESULTS_DIR}"}
	docker run --name benchmark.client --rm --network=host `
		--cpus $GRPC_CLIENT_CPUS `
        -e BENCHCLIENT_USE_S3="0" `
        benchmark.client:latest `
            BenchmarkRunner `
            $BENCH_NAME `
            -duration $GRPC_BENCHMARK_DURATION `
            -concurrency $GRPC_CLIENT_CONCURRENCY `
            -connections $GRPC_CLIENT_CONNECTIONS > "${RESULTS_DIR}/${NAME}.report"
    # convert crlf -> lf
    $output = (Get-Content -Raw "${RESULTS_DIR}/${NAME}.report") -replace "`r`n", "`n"
    [System.IO.File]::WriteAllText("$CWD/${RESULTS_DIR}/${NAME}.report", $output, $(New-Object System.Text.UTF8Encoding))
	cat "${RESULTS_DIR}/${NAME}.report" | Where-Object {$_ -match "Requests/sec"}

	Stop-Job $job
    Remove-Job $job
	docker container stop "${NAME}" > $null
}

. ./analyze.ps1 $RESULTS_DIR

echo "All done."