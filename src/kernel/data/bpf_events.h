/**
 * Contains utility for events types declarations
 * 
 * @author Yekuuun
 */

#ifndef BPF_EVENTS_H
#define BPF_EVENTS_H

#include "../includes/bpf_config.h"

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  EVENT TYPES
// └────────────────────────────────────┘
//----------------------------------------------------
//TO DO : EVENT BASED LOGS.
#define EVENT_GLOBAL 1

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  PAYLOAD ENTITIES
// └────────────────────────────────────┘ 
//----------------------------------------------------
typedef struct ebpf_event_hdr {
    __u8  type;
    __u16 size;
    __u32 timestamp;
} __attribute__((packed)) ebpf_event_hdr;

/**
 * Utility function for setting hdr attributes.
 */
#define SET_EVENT_HDR(_type, _size, _timestamp) \
((ebpf_event_hdr){                              \
    .type      = (_type),                       \
    .size      = (_size),                       \
    .timestamp = (_timestamp)                   \
})   

/**
 * Global event struct payload.
 */
typedef struct ebpf_event {
    struct ebpf_event_hdr hdr;

    // union common {
    //     /*TO DO : CREATE PAYLOADS BASED ON EVENTS. */
    // } payload;

} __attribute__((packed)) ebpf_event;

#endif