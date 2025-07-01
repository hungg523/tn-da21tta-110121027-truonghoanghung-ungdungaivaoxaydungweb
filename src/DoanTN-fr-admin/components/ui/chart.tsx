import type * as React from "react"

type ChartProps = {
  data: any[]
  children: React.ReactNode
}

export const Chart = ({ data, children }: ChartProps) => {
  return <>{children}</>
}

export const ChartTooltip = ({ children }: { children: React.ReactNode }) => {
  return <>{children}</>
}

export const ChartTooltipContent = () => {
  return null
}

export const ChartTooltipItem = () => {
  return null
}

export const ChartLegend = ({
  children,
  verticalAlign,
  align,
  iconType,
  iconSize,
  layout,
}: {
  children: React.ReactNode
  verticalAlign?: string
  align?: string
  iconType?: string
  iconSize?: number
  layout?: string
}) => {
  return <>{children}</>
}

export const ChartLegendItem = ({
  value,
  color,
}: {
  value: string
  color: string
}) => {
  return null
}

export const ChartGrid = ({
  x,
  y,
}: {
  x?: { strokeDasharray: string }
  y?: { strokeDasharray: string }
}) => {
  return null
}

export const ChartLine = ({
  dataKey,
  stroke,
  strokeWidth,
}: {
  dataKey: string
  stroke: string
  strokeWidth: number
}) => {
  return null
}

export const ChartArea = ({
  dataKey,
  fill,
  fillOpacity,
}: {
  dataKey: string
  fill: string
  fillOpacity: number
}) => {
  return null
}

export const ChartBar = ({
  dataKey,
  fill,
  radius,
  stackId,
}: {
  dataKey: string
  fill: string
  radius: number[]
  stackId?: string
}) => {
  return null
}

export const ChartXAxis = ({
  tickCount,
  tickFormatter,
}: {
  tickCount: number
  tickFormatter: (value: any) => string
}) => {
  return null
}

export const ChartYAxis = ({
  tickCount,
  tickFormatter,
}: {
  tickCount: number
  tickFormatter?: (value: any) => string
}) => {
  return null
}

export const ChartContainer = ({ children }: { children: React.ReactNode }) => {
  return <>{children}</>
}
