

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Tue Jan 29 08:48:52 2019
 */
/* Compiler settings for C:\Users\evgeny\AppData\Local\Temp\PhoneVoIPApp.BackEnd.OutOfProcess.idl-5b92719d:
    Oicf, W1, Zp8, env=Win32 (32b run), target_arch=ARM 8.00.0603 
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __PhoneVoIPApp2EBackEnd2EOutOfProcess_h__
#define __PhoneVoIPApp2EBackEnd2EOutOfProcess_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef ____x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_FWD_DEFINED__
#define ____x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_FWD_DEFINED__
typedef interface __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals;

#ifdef __cplusplus
namespace ABI {
    namespace PhoneVoIPApp {
        namespace BackEnd {
            namespace OutOfProcess {
                interface __IServerPublicNonVirtuals;
            } /* end namespace */
        } /* end namespace */
    } /* end namespace */
} /* end namespace */

#endif /* __cplusplus */

#endif 	/* ____x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_FWD_DEFINED__ */


/* header files for imported files */
#include "inspectable.h"
#include "AsyncInfo.h"
#include "EventToken.h"
#include "Windows.Foundation.h"
#include "PhoneVoIPApp.BackEnd.h"

#ifdef __cplusplus
extern "C"{
#endif 


/* interface __MIDL_itf_PhoneVoIPApp2EBackEnd2EOutOfProcess_0000_0000 */
/* [local] */ 

#if defined(__cplusplus)
}
#endif // defined(__cplusplus)
#include <Windows.Foundation.h>
#if !defined(__phonevoipapp2Ebackend_h__)
#include <PhoneVoIPApp.BackEnd.h>
#endif // !defined(__phonevoipapp2Ebackend_h__)
#if defined(__cplusplus)
extern "C" {
#endif // defined(__cplusplus)
#ifdef __cplusplus
namespace ABI {
namespace PhoneVoIPApp {
namespace BackEnd {
class CallController;
} /*BackEnd*/
} /*PhoneVoIPApp*/
}
#endif


#ifdef __cplusplus
namespace ABI {
namespace PhoneVoIPApp {
namespace BackEnd {
namespace OutOfProcess {
class Server;
} /*OutOfProcess*/
} /*BackEnd*/
} /*PhoneVoIPApp*/
}
#endif
#if !defined(____x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_INTERFACE_DEFINED__)
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_PhoneVoIPApp_BackEnd_OutOfProcess___IServerPublicNonVirtuals[] = L"PhoneVoIPApp.BackEnd.OutOfProcess.__IServerPublicNonVirtuals";
#endif /* !defined(____x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_INTERFACE_DEFINED__) */


/* interface __MIDL_itf_PhoneVoIPApp2EBackEnd2EOutOfProcess_0000_0000 */
/* [local] */ 





extern RPC_IF_HANDLE __MIDL_itf_PhoneVoIPApp2EBackEnd2EOutOfProcess_0000_0000_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_PhoneVoIPApp2EBackEnd2EOutOfProcess_0000_0000_v0_0_s_ifspec;

#ifndef ____x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_INTERFACE_DEFINED__
#define ____x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_INTERFACE_DEFINED__

/* interface __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals */
/* [uuid][object] */ 



/* interface ABI::PhoneVoIPApp::BackEnd::OutOfProcess::__IServerPublicNonVirtuals */
/* [uuid][object] */ 


EXTERN_C const IID IID___x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals;

#if defined(__cplusplus) && !defined(CINTERFACE)
    } /* end extern "C" */
    namespace ABI {
        namespace PhoneVoIPApp {
            namespace BackEnd {
                namespace OutOfProcess {
                    
                    MIDL_INTERFACE("7BF79491-56BE-375A-BC22-0058B158F01F")
                    __IServerPublicNonVirtuals : public IInspectable
                    {
                    public:
                        virtual /* [propget] */ HRESULT STDMETHODCALLTYPE get_CallController( 
                            /* [out][retval] */ ABI::PhoneVoIPApp::BackEnd::__ICallControllerPublicNonVirtuals **__returnValue) = 0;
                        
                    };

                    extern const __declspec(selectany) IID & IID___IServerPublicNonVirtuals = __uuidof(__IServerPublicNonVirtuals);

                    
                }  /* end namespace */
            }  /* end namespace */
        }  /* end namespace */
    }  /* end namespace */
    extern "C" { 
    
#else 	/* C style interface */

    typedef struct __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtualsVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetIids )( 
            __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals * This,
            /* [out] */ ULONG *iidCount,
            /* [size_is][size_is][out] */ IID **iids);
        
        HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( 
            __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals * This,
            /* [out] */ HSTRING *className);
        
        HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( 
            __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals * This,
            /* [out] */ TrustLevel *trustLevel);
        
        /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_CallController )( 
            __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals * This,
            /* [out][retval] */ __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals **__returnValue);
        
        END_INTERFACE
    } __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtualsVtbl;

    interface __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals
    {
        CONST_VTBL struct __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtualsVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_get_CallController(This,__returnValue)	\
    ( (This)->lpVtbl -> get_CallController(This,__returnValue) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* ____x_ABI_CPhoneVoIPApp_CBackEnd_COutOfProcess_C____IServerPublicNonVirtuals_INTERFACE_DEFINED__ */


/* interface __MIDL_itf_PhoneVoIPApp2EBackEnd2EOutOfProcess_0000_0001 */
/* [local] */ 

#ifndef RUNTIMECLASS_PhoneVoIPApp_BackEnd_OutOfProcess_Server_DEFINED
#define RUNTIMECLASS_PhoneVoIPApp_BackEnd_OutOfProcess_Server_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_PhoneVoIPApp_BackEnd_OutOfProcess_Server[] = L"PhoneVoIPApp.BackEnd.OutOfProcess.Server";
#endif


/* interface __MIDL_itf_PhoneVoIPApp2EBackEnd2EOutOfProcess_0000_0001 */
/* [local] */ 



extern RPC_IF_HANDLE __MIDL_itf_PhoneVoIPApp2EBackEnd2EOutOfProcess_0000_0001_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_PhoneVoIPApp2EBackEnd2EOutOfProcess_0000_0001_v0_0_s_ifspec;

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


