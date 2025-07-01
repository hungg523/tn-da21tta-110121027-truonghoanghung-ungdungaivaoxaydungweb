"use client"

import { useState, useEffect, useRef } from "react"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import Swal from 'sweetalert2'
import { aiService, Prompt, CreatePromptRequest, UpdatePromptRequest, UserActivityMetrics, ProductActivityMetrics, ModelPerformanceMetrics, ScheduleResponse, MessageResponse, ModelFile } from "@/services/ai.service"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, Tooltip, Legend, ResponsiveContainer } from "recharts"
import { Users, BarChart3, Activity, Award, Clock, Calendar } from "lucide-react"
import { Progress } from "@/components/ui/progress"
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select"
import { format, set } from "date-fns"
import { parseISO } from "date-fns"
import { environment } from '@/environments/environment';

const COLORS = ["#4f46e5", "#e5e7eb", "#22c55e", "#f59e42", "#6366f1"]

export default function AIDashboardPage() {
  const [prompts, setPrompts] = useState<Prompt[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [selectedPrompt, setSelectedPrompt] = useState<Prompt | null>(null)
  const [formData, setFormData] = useState<CreatePromptRequest>({
    name: "",
    content: ""
  })
  const [userMetrics, setUserMetrics] = useState<UserActivityMetrics | null>(null)
  const [productMetrics, setProductMetrics] = useState<ProductActivityMetrics | null>(null)
  const [modelMetrics, setModelMetrics] = useState<ModelPerformanceMetrics | null>(null)

  // State for AI Training tab
  const [trainingProgress, setTrainingProgress] = useState(0)
  const [trainingMessage, setTrainingMessage] = useState("")
  const [trainingLogs, setTrainingLogs] = useState<string[]>([])
  const [trainingMetrics, setTrainingMetrics] = useState<any>(null)
  const [isTraining, setIsTraining] = useState(false)
  const eventSourceRef = useRef<EventSource | null>(null)

  // State for AI Schedule
  const [schedule, setSchedule] = useState<ScheduleResponse | null>(null)
  const [scheduleType, setScheduleType] = useState("manual") // manual, daily, weekly, monthly
  const [scheduleTime, setScheduleTime] = useState("02:00") // HH:mm

  const [tab, setTab] = useState("metrics")

  const logRef = useRef<HTMLDivElement>(null);
  useEffect(() => {
    if (logRef.current) {
      logRef.current.scrollTop = logRef.current.scrollHeight;
    }
  }, [trainingLogs]);

  useEffect(() => {
    loadData()
  }, [])

  // Load schedule when component mounts or tab changes to training
  useEffect(() => {
    if (tab === 'training') {
      loadSchedule()
    }
  }, [tab])

  const loadData = async () => {
    setIsLoading(true)
    try {
      const [user, product, model] = await Promise.all([
        aiService.getUserActivityMetrics(),
        aiService.getProductActivityMetrics(),
        aiService.getModelPerformanceMetrics()
      ])
      setUserMetrics(user)
      setProductMetrics(product)
      setModelMetrics(model)
    } catch (e) {
      console.error('Error loading AI metrics:', e);
    } finally {
      setIsLoading(false)
    }
  }

  const loadSchedule = async () => {
    try {
      const currentSchedule = await aiService.getSchedule()
      setSchedule(currentSchedule)
      // Try to parse the cron expression back to UI state
      if (currentSchedule?.cron) {
         // Simple parsing for common cases
         const cronParts = currentSchedule.cron.split(' ');
         if (cronParts.length === 5) { // minute hour day_of_month month day_of_week
            const [minute, hour, dayOfMonth, month, dayOfWeek] = cronParts;
            setScheduleTime(`${hour.padStart(2, '0')}:${minute.padStart(2, '0')}`);

            if (dayOfMonth === '*' && month === '*' && dayOfWeek === '*') {
              setScheduleType('daily');
            } else if (dayOfMonth === '*' && month === '*' && dayOfWeek !== '*' && dayOfWeek !== '?') { // assuming numeric day of week 0-6 or 1-7
               setScheduleType('weekly'); // Basic weekly detection, might need refinement
            } else if (dayOfMonth !== '*' && dayOfMonth !== '?' && month === '*' && dayOfWeek === '?') { // assuming day of month is set, day of week is ignored
              setScheduleType('monthly'); // Basic monthly detection, might need refinement
            } else {
               setScheduleType('manual'); // Cannot parse automatically
            }
         } else {
            setScheduleType('manual'); // Cannot parse automatically
         }
      } else {
         setScheduleType('manual'); // No schedule set
      }

    } catch (error) {
      console.error('Error loading schedule:', error)
      setSchedule(null); // Clear schedule on error
      setScheduleType('manual');
      setScheduleTime('02:00');
    }
  }

  const handleCreate = async () => {
    try {
      if (formData.name && formData.name.length > 128) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Tên prompt không được vượt quá 128 ký tự'
        })
        return
      }
      if (formData.content.length > 1000) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Nội dung không được vượt quá 1000 ký tự'
        })
        return
      }

      await aiService.createPrompt(formData)
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Tạo prompt thành công'
      })
      setIsCreateDialogOpen(false)
      setFormData({
        name: "",
        content: ""
      })
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tạo prompt'
      })
    }
  }

  const handleEdit = async () => {
    if (!selectedPrompt) return

    try {
      if (formData.name && formData.name.length > 128) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Tên prompt không được vượt quá 128 ký tự'
        })
        return
      }
      if (formData.content.length > 1000) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Nội dung không được vượt quá 1000 ký tự'
        })
        return
      }

      await aiService.updatePrompt(selectedPrompt.id, formData)
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Cập nhật prompt thành công'
      })
      setIsEditDialogOpen(false)
      setSelectedPrompt(null)
      setFormData({
        name: "",
        content: ""
      })
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể cập nhật prompt'
      })
    }
  }

  const handleDelete = async (id: number) => {
    const result = await Swal.fire({
      title: 'Xác nhận xóa',
      text: 'Bạn có chắc chắn muốn xóa prompt này?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy'
    })

    if (result.isConfirmed) {
      try {
        await aiService.deletePrompt(id)
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: 'Xóa prompt thành công'
        })
        loadData()
      } catch (error) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể xóa prompt'
        })
      }
    }
  }

  const openEditDialog = (prompt: Prompt) => {
    setSelectedPrompt(prompt)
    setFormData({
      name: prompt.name || "",
      content: prompt.content
    })
    setIsEditDialogOpen(true)
  }

  // Training functions
  const startTraining = async () => {
    setIsTraining(true)
    setTrainingProgress(0)
    setTrainingMessage("Đang gửi yêu cầu training...")
    setTrainingLogs([])
    setTrainingMetrics(null)

    try {
      await aiService.triggerTrain();
      setTrainingMessage("Yêu cầu training đã được gửi. Đang chờ kết nối stream...")

      console.log("Tạo EventSource...");
      const eventSource = new EventSource(`${environment.apiAI}/admin/train-model/stream`)
      eventSourceRef.current = eventSource
      eventSource.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data)
          // Bỏ qua heartbeat messages
          if (data.type === "heartbeat") return

          // Luôn push log nếu có message
          if (data.message) {
            setTrainingLogs(prev => [...prev, data.message])
            setTrainingMessage(data.message)
          }

          if (typeof data.progress === "number") {
            setTrainingProgress(data.progress)
            if (data.progress === 100) {
              setTimeout(() => {
                eventSource.close()
                setIsTraining(false)
              }, 1000)
            }
          }

          if (data.metrics) {
            setTrainingMetrics(data.metrics)
            setTimeout(() => {
              eventSource.close()
              setIsTraining(false)
            }, 1000)
          }

          if (data.error) {
            setTrainingLogs(prev => [...prev, `Lỗi: ${data.error}`])
            eventSource.close()
            setIsTraining(false)
          }
        } catch (error) {
          console.error('Error parsing SSE data:', error)
          setTrainingLogs(prev => [...prev, `Error receiving data: ${event.data}`])
        }
      }
    } catch (error) {
      console.error('Error triggering training:', error);
      setIsTraining(false);
    }
  }

  const stopTraining = () => {
    if (eventSourceRef.current) {
      eventSourceRef.current.close()
      setIsTraining(false)
      setTrainingMessage("Đã dừng tiến trình training.")
      setTrainingLogs((prev) => [...prev, "Đã dừng tiến trình training."])
    }
  }

  // Schedule management functions
  const handleScheduleTrain = async () => {

    let cronExpression = '';
    const [hours, minutes] = scheduleTime.split(':');

    switch(scheduleType) {
      case 'manual':
        Swal.fire('Thông báo', 'Vui lòng chọn loại lịch', 'info');
        return;
      case 'daily':
        cronExpression = `${parseInt(minutes)} ${parseInt(hours)} * * *`;
        break;
      case 'weekly':
         // Simple weekly: e.g., Monday at specified time (day of week 1)
         // Need more UI for selecting day of week if required.
         // For simplicity, assuming Monday (day of week 1)
         cronExpression = `${parseInt(minutes)} ${parseInt(hours)} * * 1`;
         break;
      case 'monthly':
         // Simple monthly: e.g., 1st day of month at specified time (day of month 1)
         // Need more UI for selecting day of month if required.
         // For simplicity, assuming 1st day
         cronExpression = `${parseInt(minutes)} ${parseInt(hours)} 1 * *`;
         break;
    }

    if (!cronExpression) {
       Swal.fire('Lỗi', 'Không thể tạo Cron Expression từ lựa chọn.', 'error');
       return;
    }

    try {
      const result = await aiService.scheduleTrain(cronExpression);
      Swal.fire('Thành công', result.message, 'success');
      loadSchedule(); // Reload schedule after setting
    } catch (error: any) {
      Swal.fire('Lỗi', error.message, 'error');
    }
  }

  const handleGetSchedule = async () => {
     loadSchedule();
  }

  const handleDeleteSchedule = async () => {
    try {
      const result = await aiService.deleteSchedule();
      Swal.fire('Thành công', result.message, 'success');
      loadSchedule(); // Reload schedule after deleting
    } catch (error: any) {
      Swal.fire('Lỗi', error.message, 'error');
    }
  }


  // Pie data for user activity
  const userPieData = userMetrics ? [
    { name: "Active", value: userMetrics.active_users },
    { name: "Inactive", value: userMetrics.inactive_users }
  ] : []

  // Bar data for product activity
  const productBarData = productMetrics ? [
    { name: "Popular", value: productMetrics.popular_products },
    { name: "Unpopular", value: productMetrics.unpopular_products }
  ] : []

  // Bar data for model performance
  const modelBarData = modelMetrics ? [
    { name: "Precision@k", value: modelMetrics["precision@k"] },
    { name: "Recall@k", value: modelMetrics["recall@k"] },
    { name: "MAP@k", value: modelMetrics["map@k"] }
  ] : []

  // Column chart for user/product interactions
  const userInteractionData = userMetrics ? [
    { name: "Min", value: userMetrics.min_interactions_per_user },
    { name: "Avg", value: userMetrics.avg_interactions_per_user },
    { name: "Max", value: userMetrics.max_interactions_per_user }
  ] : []
  const productInteractionData = productMetrics ? [
    { name: "Min", value: productMetrics.min_interactions_per_product },
    { name: "Avg", value: productMetrics.avg_interactions_per_product },
    { name: "Max", value: productMetrics.max_interactions_per_product }
  ] : []

  // Time options for schedule
  const timeOptions = Array.from({ length: 24 * 4 }, (_, i) => {
    const hours = Math.floor(i / 4);
    const minutes = (i % 4) * 15;
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
  });

  // State cho quản lý file model
  const [modelFiles, setModelFiles] = useState<ModelFile[]>([])
  const [isLoadingModels, setIsLoadingModels] = useState(false)
  const [currentModel, setCurrentModel] = useState<string | null>(null)

  // Load danh sách file model khi vào tab training
  useEffect(() => {
    if (tab === 'training') {
      loadModelFiles()
    }
  }, [tab])

  const loadModelFiles = async () => {
    setIsLoadingModels(true)
    try {
      const files = await aiService.getAllModelFiles()
      setModelFiles(files)
      // Nếu backend có API lấy model đang dùng thì gọi, tạm thời lấy file mới nhất
      if (files.length > 0) setCurrentModel(files[files.length-1].filename)
    } catch (e: any) {
      Swal.fire('Lỗi', e.message || 'Không thể tải danh sách file model', 'error')
    } finally {
      setIsLoadingModels(false)
    }
  }

  const handleUseModel = async (filename: string) => {
    try {
      await aiService.useModelFile(filename)
      Swal.fire('Thành công', `Đã chọn model: ${filename}`, 'success')
      setCurrentModel(filename)
    } catch (e: any) {
      Swal.fire('Lỗi', e.message || 'Không thể chọn model', 'error')
    }
  }

  const handleDeleteModel = async (filename: string) => {
    const result = await Swal.fire({
      title: 'Xác nhận xóa',
      text: `Bạn có chắc chắn muốn xóa model ${filename}?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy'
    })
    if (result.isConfirmed) {
      try {
        await aiService.deleteModelFile(filename)
        Swal.fire('Thành công', 'Đã xóa model', 'success')
        loadModelFiles()
      } catch (e: any) {
        Swal.fire('Lỗi', e.message || 'Không thể xóa model', 'error')
      }
    }
  }

  return (
    <div className="space-y-8">
      <h1 className="text-3xl font-bold">Dashboard AI</h1>
      <Tabs value={tab} onValueChange={setTab} className="space-y-4">
        <TabsList className="mb-4">
          <TabsTrigger value="metrics">Thống kê AI</TabsTrigger>
          <TabsTrigger value="training">Quản lý Training</TabsTrigger>
        </TabsList>
        <TabsContent value="metrics">
            <div className="space-y-4">
            <div className="grid gap-4 md:grid-cols-3 lg:grid-cols-5">
              <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium flex items-center gap-2"><Users className="h-4 w-4" /> Tổng user</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{userMetrics?.total_users ?? '-'}</div>
                  <p className="text-xs text-muted-foreground">Tổng số người dùng</p>
                </CardContent>
              </Card>
              <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium flex items-center gap-2"><Activity className="h-4 w-4" /> User hoạt động</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{userMetrics?.active_users ?? '-'}</div>
                  <p className="text-xs text-muted-foreground">Số user hoạt động</p>
                </CardContent>
              </Card>
              <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium flex items-center gap-2"><Activity className="h-4 w-4" /> User không hoạt động</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{userMetrics?.inactive_users ?? '-'}</div>
                  <p className="text-xs text-muted-foreground">Số user không hoạt động</p>
                </CardContent>
              </Card>
              <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium flex items-center gap-2"><BarChart3 className="h-4 w-4" /> Tương tác/user TB</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{userMetrics?.avg_interactions_per_user?.toFixed(1) ?? '-'}</div>
                  <p className="text-xs text-muted-foreground">Tương tác trung bình/user</p>
                </CardContent>
              </Card>
              <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium flex items-center gap-2"><BarChart3 className="h-4 w-4" /> Tương tác/user (min-max)</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{userMetrics ? `${userMetrics.min_interactions_per_user} - ${userMetrics.max_interactions_per_user}` : '-'}</div>
                  <p className="text-xs text-muted-foreground">Tương tác thấp nhất/cao nhất</p>
                </CardContent>
              </Card>
            </div>
            <div className="grid gap-4 md:grid-cols-3">
              <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium flex items-center gap-2"><BarChart3 className="h-4 w-4" /> Hiệu suất mô hình</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="flex flex-col gap-2">
                    <div>Precision@k: <span className="font-bold">{modelMetrics?.["precision@k"]?.toFixed(3) ?? '-'}</span></div>
                    <div>Recall@k: <span className="font-bold">{modelMetrics?.["recall@k"]?.toFixed(3) ?? '-'}</span></div>
                    <div>MAP@k: <span className="font-bold">{modelMetrics?.["map@k"]?.toFixed(3) ?? '-'}</span></div>
                  </div>
                </CardContent>
              </Card>
            </div>
            <div className="grid gap-4 md:grid-cols-2">
              <Card>
                <CardHeader>
                  <CardTitle>Người dùng hoạt động và không hoạt động</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="h-[250px]">
                    <ResponsiveContainer width="100%" height="100%">
                      <PieChart>
                        <Pie data={userPieData} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={80} label>
                          {userPieData.map((entry, idx) => (
                            <Cell key={`cell-${idx}`} fill={COLORS[idx % COLORS.length]} />
                          ))}
                        </Pie>
                        <Legend />
                        <Tooltip />
                      </PieChart>
                    </ResponsiveContainer>
              </div>
                </CardContent>
              </Card>
              <Card>
                <CardHeader>
                  <CardTitle>Sản phẩm phổ biến và ít phổ biến</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="h-[250px]">
                    <ResponsiveContainer width="100%" height="100%">
                      <BarChart data={productBarData} margin={{ top: 10, right: 30, left: 0, bottom: 0 }}>
                        <XAxis dataKey="name" />
                        <YAxis allowDecimals={false} />
                        <Tooltip />
                        <Legend />
                        <Bar dataKey="value" fill="#4f46e5" />
                      </BarChart>
                    </ResponsiveContainer>
              </div>
                </CardContent>
              </Card>
            </div>
            <div className="grid gap-4 md:grid-cols-2">
              <Card>
                <CardHeader>
                  <CardTitle>Tương tác/user (min, avg, max)</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="h-[250px]">
                    <ResponsiveContainer width="100%" height="100%">
                      <BarChart data={userInteractionData} margin={{ top: 10, right: 30, left: 0, bottom: 0 }}>
                        <XAxis dataKey="name" />
                        <YAxis allowDecimals={false} />
                        <Tooltip />
                        <Legend />
                        <Bar dataKey="value" fill="#22c55e" />
                      </BarChart>
                    </ResponsiveContainer>
                  </div>
                </CardContent>
              </Card>
              <Card>
                <CardHeader>
                  <CardTitle>Tương tác/sản phẩm (min, avg, max)</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="h-[250px]">
                    <ResponsiveContainer width="100%" height="100%">
                      <BarChart data={productInteractionData} margin={{ top: 10, right: 30, left: 0, bottom: 0 }}>
                        <XAxis dataKey="name" />
                        <YAxis allowDecimals={false} />
                        <Tooltip />
                        <Legend />
                        <Bar dataKey="value" fill="#f59e42" />
                      </BarChart>
                    </ResponsiveContainer>
      </div>
                </CardContent>
              </Card>
            </div>
            <div className="grid gap-4 md:grid-cols-1">
              <Card>
                <CardHeader>
                  <CardTitle>Hiệu suất mô hình (Precision, Recall, MAP)</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="h-[250px]">
                    <ResponsiveContainer width="100%" height="100%">
                      <BarChart data={modelBarData} margin={{ top: 10, right: 30, left: 0, bottom: 0 }}>
                        <XAxis dataKey="name" />
                        <YAxis />
                        <Tooltip />
                        <Legend />
                        <Bar dataKey="value" fill="#6366f1" />
                      </BarChart>
                    </ResponsiveContainer>
                  </div>
                </CardContent>
              </Card>
            </div>
          </div>
        </TabsContent>
        <TabsContent value="training">
          <div className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Tiến trình Training Model AI</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="flex items-center gap-4 mb-4">
                  <Button onClick={startTraining} disabled={isTraining}>
                    {isTraining ? "Đang training..." : "Train model ngay"}
                  </Button>
                  {isTraining && (
                    <Button variant="destructive" onClick={stopTraining}>
                      Dừng
                    </Button>
                  )}
                </div>
                <Progress value={trainingProgress} className="h-3 mb-2" />
                <div className="text-sm mb-2">{trainingMessage}</div>
                <div
                  className="bg-black p-3 rounded h-48 overflow-y-auto text-xs"
                  style={{ fontFamily: "monospace" }}
                  ref={logRef}
                >
                  {trainingLogs.map((line, idx) => {
                    let color = "text-gray-100";
                    if (/error|failed/i.test(line)) color = "text-red-400";
                    else if (/success|completed/i.test(line)) color = "text-emerald-400";
                    else if (/saving|saved/i.test(line)) color = "text-blue-400";
                    else if (/precision|recall|map/i.test(line)) color = "text-yellow-300";
                    return (
                      <div key={idx} className={color}>
                        {line}
                      </div>
                    );
                  })}
                </div>
                {trainingMetrics && (
                  <div className="mt-4 bg-gray-100 p-3 rounded">
                    <div className="font-semibold mb-2">Kết quả training:</div>
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <div className="text-sm font-medium">Precision@k:</div>
                        <div className="text-lg font-bold">{trainingMetrics["precision@k"]?.toFixed(3) ?? '-'}</div>
                      </div>
                      <div>
                        <div className="text-sm font-medium">Recall@k:</div>
                        <div className="text-lg font-bold">{trainingMetrics["recall@k"]?.toFixed(3) ?? '-'}</div>
                      </div>
                      <div>
                        <div className="text-sm font-medium">MAP@k:</div>
                        <div className="text-lg font-bold">{trainingMetrics["map@k"]?.toFixed(3) ?? '-'}</div>
                      </div>
                    </div>
                  </div>
                )}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Lịch Train Model Tự động</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <div className="font-semibold mb-2">Lịch hiện tại:</div>
                  {schedule ? (
                    <div className="text-sm">Cron: <Badge>{schedule.cron || 'Chưa đặt'}</Badge>, Lần chạy kế tiếp: <Badge>{schedule.next_run_time ? format(parseISO(schedule.next_run_time), 'dd/MM/yyyy HH:mm') : 'Chưa có'}</Badge></div>
                  ) : (
                    <p className="text-sm text-muted-foreground">Đang tải hoặc chưa có lịch nào được đặt.</p>
                  )}
                </div>
                <div className="flex gap-2 items-end">
                   <div className="flex flex-col gap-2">
                     <label className="text-sm font-medium">Tần suất</label>
                      <Select value={scheduleType} onValueChange={setScheduleType}>
                         <SelectTrigger className="w-[180px]">
                           <SelectValue placeholder="Chọn tần suất" />
                         </SelectTrigger>
                         <SelectContent>
                            <SelectItem value="manual">Thủ công</SelectItem>
                            <SelectItem value="daily">Hàng ngày</SelectItem>
                            <SelectItem value="weekly">Hàng tuần</SelectItem>
                            <SelectItem value="monthly">Hàng tháng</SelectItem>
                         </SelectContent>
                      </Select>
                   </div>
                    <div className="flex flex-col gap-2">
                       <label className="text-sm font-medium">Thời gian (HH:mm)</label>
                         <Select value={scheduleTime} onValueChange={setScheduleTime}>
                           <SelectTrigger className="w-[120px]">
                              <SelectValue placeholder="Chọn giờ" />
                           </SelectTrigger>
                           <SelectContent>
                             {timeOptions.map(time => (
                               <SelectItem key={time} value={time}>{time}</SelectItem>
                             ))}
                           </SelectContent>
                         </Select>
                    </div>
                  <Button onClick={handleScheduleTrain} disabled={scheduleType === 'manual' || isTraining}>Đặt lịch</Button>
                  <Button variant="outline" onClick={handleGetSchedule}>Refresh</Button>
                  {schedule && schedule.cron && (
                    <Button variant="destructive" onClick={handleDeleteSchedule} disabled={isTraining}>Hủy lịch</Button>
                  )}
                </div>
                <div className="text-xs text-muted-foreground">
                   Lưu ý: Lịch hàng tuần/tháng mặc định chạy vào Thứ 2/Ngày 1 hàng tuần/tháng. Cần tùy chỉnh Cron Expression chi tiết hơn nếu muốn thay đổi.
                </div>
              </CardContent>
            </Card>

            {/* Quản lý file model */}
            <Card>
              <CardHeader>
                <CardTitle>Quản lý file Model AI</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="mb-2 text-sm">
                  Model đang sử dụng: <span className="font-semibold text-primary">{currentModel || 'Chưa chọn'}</span>
                </div>
                <div className="overflow-x-auto">
                  <table className="min-w-full text-xs border">
                    <thead>
                      <tr className="bg-gray-100">
                        <th className="px-2 py-1 border">Tên file</th>
                        <th className="px-2 py-1 border">Ngày tạo</th>
                        <th className="px-2 py-1 border">Dung lượng</th>
                        <th className="px-2 py-1 border">Thao tác</th>
                      </tr>
                    </thead>
                    <tbody>
                      {isLoadingModels ? (
                        <tr><td colSpan={4} className="text-center py-2">Đang tải...</td></tr>
                      ) : modelFiles.length === 0 ? (
                        <tr><td colSpan={4} className="text-center py-2">Chưa có file model nào</td></tr>
                      ) : modelFiles.map(file => (
                        <tr key={file.filename} className={file.filename === currentModel ? 'bg-green-50' : ''}>
                          <td className="px-2 py-1 border font-mono">{file.filename}</td>
                          <td className="px-2 py-1 border">{new Date(file.created * 1000).toLocaleString()}</td>
                          <td className="px-2 py-1 border">{(file.size/1024).toFixed(1)} KB</td>
                          <td className="px-2 py-1 border space-x-2">
                            <Button size="sm" variant={file.filename === currentModel ? 'default' : 'outline'} disabled={file.filename === currentModel} onClick={() => handleUseModel(file.filename)}>
                              Sử dụng
                    </Button>
                            <Button size="sm" variant="destructive" onClick={() => handleDeleteModel(file.filename)}>
                      Xóa
                    </Button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </CardContent>
            </Card>

                  </div>
        </TabsContent>
      </Tabs>
    </div>
  )
} 