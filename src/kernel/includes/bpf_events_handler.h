/**
 * Utility class for handling ringbuffer events sending process.
 * 
 * @author Yekuuun
 */

#ifndef BPF_EVENTS_HANDLER_H
#define BPF_EVENTS_HANDLER_H

#include "../includes/bpf_config.h"
#include "../includes/bpf_common.h"
#include "../data/bpf_events.h"
#include "../data/bpf_ringbuf.h"
#include "../lib/ftlib.h"

/**
 * Main function to handle event sending.
 * 
 * @param event_type => event type 
 * @param payload => payload containing informations.
 * 
 * @return => 0 if success, 1 if error.
 */
static __always_inline void send_event(short event_type, const char *desc, const char *name)
{
    if(!desc || !name)
        return;

    ebpf_event *evt = bpf_ringbuf_reserve(&event_output, sizeof(ebpf_event), 0);
    if(!evt)
        return;

    __rtl_secure_zero_memory(evt, sizeof(ebpf_event));
    evt->hdr = SET_EVENT_HDR(event_type, (__u16)sizeof(ebpf_event), bpf_ktime_get_ns());

    bpf_probe_read_kernel_str(evt->payload.event_desc, sizeof(evt->payload.event_desc), desc);
    bpf_probe_read_kernel_str(evt->payload.event_name, sizeof(evt->payload.event_name), name);

    bpf_ringbuf_submit(evt, 0);
}

#endif