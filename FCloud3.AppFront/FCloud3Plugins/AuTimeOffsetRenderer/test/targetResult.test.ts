import { describe, it, expect } from 'vitest'
import {
  getTargetResult,
  msToTimeOffset,
  timeOffsetToString,
  padTo2Digits,
  isSameDay
} from '../lib/targetRenderer/targetResult'
import type { Target } from '../lib/targetParser/target'

describe('targetResult', () => {
  describe('padTo2Digits', () => {
    it('should pad single digit', () => {
      expect(padTo2Digits(0)).toBe('00')
      expect(padTo2Digits(5)).toBe('05')
      expect(padTo2Digits(9)).toBe('09')
    })

    it('should not pad double digit', () => {
      expect(padTo2Digits(10)).toBe('10')
      expect(padTo2Digits(59)).toBe('59')
    })
  })

  describe('isSameDay', () => {
    it('should return true for same day', () => {
      const d1 = new Date(2025, 5, 15, 8, 0, 0)
      const d2 = new Date(2025, 5, 15, 20, 30, 0)
      expect(isSameDay(d1, d2)).toBe(true)
    })

    it('should return false for different day', () => {
      const d1 = new Date(2025, 5, 15)
      const d2 = new Date(2025, 5, 16)
      expect(isSameDay(d1, d2)).toBe(false)
    })

    it('should return false for different month', () => {
      const d1 = new Date(2025, 5, 15)
      const d2 = new Date(2025, 6, 15)
      expect(isSameDay(d1, d2)).toBe(false)
    })
  })

  describe('msToTimeOffset', () => {
    it('should convert milliseconds to time offset', () => {
      const result = msToTimeOffset(90061000) // 1 day + 1 hour + 1 min + 1 sec
      expect(result.day).toBe(1)
      expect(result.hour).toBe(1)
      expect(result.min).toBe(1)
      expect(result.sec).toBe(1)
    })

    it('should handle zero', () => {
      const result = msToTimeOffset(0)
      expect(result.day).toBe(0)
      expect(result.hour).toBe(0)
      expect(result.min).toBe(0)
      expect(result.sec).toBe(0)
    })

    it('should handle exact days', () => {
      const result = msToTimeOffset(86400000 * 3) // 3 days
      expect(result.day).toBe(3)
      expect(result.hour).toBe(0)
      expect(result.min).toBe(0)
      expect(result.sec).toBe(0)
    })
  })

  describe('timeOffsetToString', () => {
    it('should format without time of day', () => {
      const to = { day: 5, hour: 3, min: 2, sec: 1 }
      expect(timeOffsetToString(to, false)).toBe('6天')
    })

    it('should format with time of day', () => {
      const to = { day: 1, hour: 2, min: 5, sec: 9 }
      expect(timeOffsetToString(to, true)).toBe('1天 02:05:09')
    })

    it('should format with time of day no days', () => {
      const to = { day: 0, hour: 12, min: 30, sec: 0 }
      expect(timeOffsetToString(to, true)).toBe('12:30:00')
    })
  })

  describe('getTargetResult', () => {
    it('should show 即为今日 for same day without time', () => {
      const now = new Date(2025, 5, 15, 10, 0, 0)
      const target: Target = {
        t: new Date(2025, 5, 15, 0, 0, 0),
        specifyTimeOfDay: false,
        desc: '今天'
      }
      expect(getTargetResult(target, now)).toBe('今天 <b>即为今日</b>')
    })

    it('should show remaining time with desc', () => {
      const now = new Date(2025, 5, 15, 10, 0, 0)
      const target: Target = {
        t: new Date(2025, 5, 20, 0, 0, 0),
        specifyTimeOfDay: false,
        desc: '目标日'
      }
      const result = getTargetResult(target, now)
      expect(result).toContain('距离 目标日 剩余')
      expect(result).toContain('<b>')
    })

    it('should show passed time with desc', () => {
      const now = new Date(2025, 5, 20, 10, 0, 0)
      const target: Target = {
        t: new Date(2025, 5, 15, 0, 0, 0),
        specifyTimeOfDay: false,
        desc: '目标日'
      }
      const result = getTargetResult(target, now)
      expect(result).toContain('距离 目标日 已过')
    })

    it('should show remaining time without desc', () => {
      const now = new Date(2025, 5, 15, 10, 0, 0)
      const target: Target = {
        t: new Date(2025, 5, 20, 0, 0, 0),
        specifyTimeOfDay: false
      }
      const result = getTargetResult(target, now)
      expect(result).toContain('剩余')
    })

    it('should show passed time without desc', () => {
      const now = new Date(2025, 5, 20, 10, 0, 0)
      const target: Target = {
        t: new Date(2025, 5, 15, 0, 0, 0),
        specifyTimeOfDay: false
      }
      const result = getTargetResult(target, now)
      expect(result).toContain('已过')
    })

    it('should not show 即为今日 when time is specified', () => {
      const now = new Date(2025, 5, 15, 10, 0, 0)
      const target: Target = {
        t: new Date(2025, 5, 15, 8, 0, 0),
        specifyTimeOfDay: true,
        desc: '今天'
      }
      const result = getTargetResult(target, now)
      expect(result).not.toContain('即为今日')
      expect(result).toContain('已过')
    })

    it('should format with time precision when specified', () => {
      const now = new Date(2025, 5, 15, 10, 0, 0)
      const target: Target = {
        t: new Date(2025, 5, 15, 14, 30, 0),
        specifyTimeOfDay: true,
        desc: '下午'
      }
      const result = getTargetResult(target, now)
      expect(result).toContain('距离 下午 剩余')
      expect(result).toMatch(/\d{2}:\d{2}:\d{2}/)
    })
  })
})
