#!/bin/bash

#utility command for displaying debug output from tracing pipe.

sudo cat /sys/kernel/tracing/trace_pipe | sed -E 's/^.*bpf_trace_printk: //'